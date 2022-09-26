#!/usr/bin/python -u
"""jupiter-fan-controller"""
import signal
import os
import sys
import subprocess
import time
import math
import yaml
from PID import PID

# quadratic function RPM = AT^2 + BT + C
class Quadratic():
    '''quadratic function controller'''
    def __init__(self, A, B, C, T_threshold = 0) -> None:
        '''constructor'''
        self.A = A
        self.B = B
        self.C = C
        self.T_threshold = T_threshold
        self.output = 0
    
    def update(self, temp_input, _) -> int:
        '''update output'''
        if temp_input < self.T_threshold:
            self.output = 0
        else:
            self.output = int(self.A * math.pow(temp_input, 2) + self.B * temp_input + self.C)
        return self.output

class FeedForward():
    '''RPM predicted by APU power is fed forward + PID output stage'''
    def __init__(self, Kp, Ki, Kd, windup, winddown, a_ff, b_ff, temp_setpoint) -> None:
        '''constructor'''
        self.a_ff = a_ff
        self.b_ff = b_ff
        self.temp_setpoint = temp_setpoint
        self.pid = PID(Kp, Ki, Kd)
        self.pid.SetPoint = temp_setpoint
        self.pid.setWindup(windup)
        self.pid.setWinddown(winddown)
        self.output = 0

    def print_ff_state(self, ff_output, pid_output) -> str:
        '''prints state variables of FF and PID, helpful for debug'''
        print(f"FeedForward Controller - FF:{ff_output:.0f}    PID: {-1 * self.pid.PTerm:.0f}  {-1 * self.pid.Ki * self.pid.ITerm:.0f}  {-1 * self.pid.Kd * self.pid.DTerm:.0f} = {pid_output:.0f}")

    def get_ff_setpoint(self, power_input) -> int:
        '''returns the feed forward portion of the controller output'''
        rpm_setpoint = int(self.a_ff * power_input + self.b_ff)
        return rpm_setpoint

    def update(self, temp_input, power_input) -> int:
        '''run controller to update output'''
        pid_output = self.pid.update(temp_input)
        ff_output = self.get_ff_setpoint(power_input)
        self.output = int(pid_output + ff_output)
        # self.print_ff_state(ff_output, pid_output)
        return self.output

class FeedForwardMin():
    '''FF with an additional min curve'''
    def __init__(self, Kp, Ki, Kd, windup, winddown, a_ff, b_ff, temp_setpoint, a_min, b_min) -> None:
        '''constructor'''
        self.a_ff = a_ff
        self.b_ff = b_ff
        self.a_min = a_min
        self.b_min = b_min
        self.temp_setpoint = temp_setpoint
        self.pid = PID(Kp, Ki, Kd)
        self.pid.SetPoint = temp_setpoint
        self.pid.setWindup(windup)
        self.pid.setWinddown(winddown)
        self.output = 0

    def print_ff_state(self, ff_output, pid_output, min_setpoint) -> str:
        '''prints state variables of FF and PID, helpful for debug'''
        print(f"FeedForward Controller - Min:{min_setpoint}    FF:{ff_output:.0f}    PID:{-1 * self.pid.PTerm:.0f}/{-1 * self.pid.Ki * self.pid.ITerm:.0f}/{-1 * self.pid.Kd * self.pid.DTerm:.0f} = {ff_output + pid_output:.0f}")

    def get_ff_setpoint(self, power_input) -> int:
        '''returns the feed forward portion of the controller output'''
        rpm_setpoint = int(self.a_ff * power_input + self.b_ff)
        return rpm_setpoint

    def get_min_setpoint(self, temp_input) -> int:
        '''returns a minimum rpm speed for the given temperature'''
        rpm_setpoint =  int(self.a_min * temp_input + self.b_min)
        return rpm_setpoint

    def update(self, temp_input, power_input) -> int:
        '''run controller to update output'''
        pid_output = int(self.pid.update(temp_input))
        ff_output = self.get_ff_setpoint(power_input)
        min_setpoint = self.get_min_setpoint(temp_input)
        self.output = max(min_setpoint,(pid_output + ff_output))
        self.print_ff_state(ff_output, pid_output, min_setpoint)
        return self.output

class FeedForwardQuad():
    '''FF with an additional min curve'''
    def __init__(self, a_quad, b_quad, c_quad, a_ff, b_ff) -> None:
        '''constructor'''
        self.a_ff = a_ff
        self.b_ff = b_ff
        # self.temp_setpoint = temp_setpoint
        self.ff_deadzone = 300
        self.ff_last_setpoint = 0
        self.quad = Quadratic(a_quad, b_quad, c_quad)
        self.output = 0

    def print_ff_state(self, ff_output, quad_output):
        '''prints state variables of FF and PID, helpful for debug'''
        print(f"FeedForward Controller - Quad:{quad_output}    FF:{ff_output:.0f}")

    def get_ff_setpoint(self, power_input) -> int:
        '''returns the feed forward portion of the controller output'''
        rpm_setpoint = int(self.a_ff * power_input + self.b_ff)
        if abs(rpm_setpoint - self.ff_last_setpoint) > self.ff_deadzone:
            self.ff_last_setpoint = rpm_setpoint
            return rpm_setpoint
        return self.ff_last_setpoint

    def update(self, temp_input, power_input) -> int:
        '''run controller to update output'''
        quad_output = int(self.quad.update(temp_input, None))
        ff_output = self.get_ff_setpoint(power_input)
        # min_setpoint = self.get_min_setpoint(temp_input)
        self.output = quad_output + ff_output
        self.print_ff_state(ff_output, quad_output)
        return self.output

class Fan():
    '''fan object controls all jupiter hwmon parameters'''
    def __init__(self, fan_path, config, debug = False) -> None:
        '''constructor'''
        self.debug = debug
        self.fan_path = fan_path
        self.charge_state_path = config["charge_state_path"]
        self.min_speed = config["fan_min_speed"]
        self.threshold_speed = config["fan_threshold_speed"]
        self.max_speed = config["fan_max_speed"]
        self.gain = config["fan_gain"]
        self.ec_ramp_rate = config["ec_ramp_rate"]
        self.fc_speed = 0
        self.measured_speed = 0
        self.charge_state = False
        self.charge_min_speed = 2000
        self.has_std_bios = self.bios_compatibility_check()
        self.take_control_from_ec()
        self.set_speed(3000)

    @staticmethod
    def bios_compatibility_check() -> bool:
        """returns True for bios versions >= 106, false for earlier versions"""
        version = subprocess.check_output(["dmidecode", "-s", "bios-version"]) # b'F7A0104T06\n'
        version = int(version[3:7])

        if version >= 106:
            return True
        else:
            return False

    def take_control_from_ec(self) -> None:
        '''take over fan control from ec mcu'''
        if self.has_std_bios:
            return
        else:
            with open(self.fan_path + "gain", 'w', encoding="utf8") as f:
                f.write(str(self.gain))
            with open(self.fan_path + "ramp_rate", 'w', encoding="utf8") as f:
                f.write(str(self.ec_ramp_rate))
            with open(self.fan_path + "recalculate", 'w', encoding="utf8") as f:
                f.write(str(1))

    def return_to_ec_control(self) -> None:
        '''reset EC to generate fan values internally'''
        if self.has_std_bios:
            with open(self.fan_path + "fan1_target", 'w', encoding="utf8") as f:
                f.write(str(int(0)))
        else:
            with open(self.fan_path + "gain", 'w', encoding="utf8") as f:
                f.write(str(10))
            with open(self.fan_path + "ramp_rate", 'w', encoding="utf8") as f:
                f.write(str(20))
            with open(self.fan_path + "recalculate", 'w', encoding="utf8") as f:
                f.write(str(0))

    def get_speed(self) -> int:
        '''returns the measured (real) fan speed'''
        with open(self.fan_path + "fan1_input", 'r', encoding="utf8") as f:
            self.measured_speed = int(f.read().strip())
        return self.measured_speed

    def get_charge_state(self) -> bool:
        '''updates min rpm depending on charge state'''
        with open(self.charge_state_path, 'r', encoding="utf8") as f:
            state = f.read().strip()
        if state == "Charging":
            self.charge_state = True
        else:
            self.charge_state = False
        return self.charge_state

    def set_speed(self, speed) -> None:
        '''sets a new target speed'''
        if speed > self.max_speed:
            speed = self.max_speed
        elif self.charge_state:
            if speed < self.charge_min_speed:
                speed = self.charge_min_speed
        elif speed < self.threshold_speed:
            speed = self.min_speed
            
        self.fc_speed = speed
        with open(self.fan_path + "fan1_target", 'w', encoding="utf8") as f:
            f.write(str(int(self.fc_speed)))

class Device():
    '''devices are sources of heat - CPU, GPU, etc.'''
    def __init__(self, base_path, config, fan_max_speed, n_sample_avg, debug = False) -> None:
        '''constructor'''
        self.file_path = get_full_path(base_path, config["hwmon_name"]) + config["file"]
        self.debug = debug
        self.fan_max_speed = fan_max_speed
        self.n_sample_avg = n_sample_avg
        self.nice_name = config["nice_name"]
        self.max_temp = config["max_temp"]
        self.temp_deadzone = config["temp_deadzone"]
        self.temp = 0
        self.control_temp = 0 # deadzone temp
        self.control_output = 0 # controller output if > 0, max fan speed if max temp reached
        self.buffer_full = False
        self.control_temps = []
        self.avg_control_temp = 0

        # instantiate controller depending on type
        self.type = config["type"]
        if self.type == "pid":
            self.controller = PID(float(config["Kp"]), float(config["Ki"]), float(config["Kd"]))  
            self.controller.SetPoint = config["T_setpoint"]
            self.controller.setWindup(config["windup_limit"]) # windup limits the I term of the output
        elif self.type ==  "quadratic":
            self.controller = Quadratic(float(config["A"]), float(config["B"]), float(config["C"]), float(config["T_threshold"]))
        elif self.type == "feedforward":
            self.controller = FeedForward(float(config["Kp"]), float(config["Ki"]), float(config["Kd"]), int(config["windup"]), int(config["winddown"]), float(config["A_ff"]), float(config["B_ff"]), float(config["T_setpoint"]))
        elif self.type == "ffmin":
            self.controller = FeedForwardMin(float(config["Kp"]), float(config["Ki"]), float(config["Kd"]), int(config["windup"]), int(config["winddown"]), float(config["A_ff"]), float(config["B_ff"]), float(config["T_setpoint"]), float(config["A_min"]), float(config["B_min"]))
        elif self.type == "ffquad":
            self.controller = FeedForwardQuad(float(config["A_quad"]), float(config["B_quad"]), float(config["C_quad"]), float(config["A_ff"]), float(config["B_ff"]))
        else:
            print("error loading device controller \n")
            exit(1)

    def get_temp(self) -> None:
        '''updates temperatures'''
        with open(self.file_path, 'r', encoding="utf8") as f:
            self.temp = int(f.read().strip()) / 1000
        # only update the control temp if it's outside temp_deadzone
        if math.fabs(self.temp - self.control_temp) >= self.temp_deadzone:
            self.control_temp = self.temp
        return self.control_temp

    def get_avg_temp(self):
        '''updates temperature list + generates average value'''
        self.control_temps.append(self.get_temp())
        if self.buffer_full:
            self.control_temps.pop(0)
        elif len(self.control_temps) >= self.n_sample_avg:
            self.buffer_full = True
        self.avg_control_temp = math.fsum(self.control_temps) / len(self.control_temps)
        return self.avg_control_temp

    def get_output(self, temp_input, power_input) -> int:
        '''updates the device controller and returns bounded output'''
        self.controller.update(temp_input, power_input)
        self.control_output = max(self.controller.output, 0)
        if(temp_input > self.max_temp):
            self.control_output = self.fan_max_speed
        return self.control_output

class Sensor():
    '''sensor for measuring non-temperature values'''
    def __init__(self, base_path, config, debug = False) -> None:
        self.file_path = get_full_path(base_path, config["hwmon_name"]) + config["file"]
        self.debug = debug
        self.nice_name = config["nice_name"]
        self.n_sample_avg = config["n_sample_avg"]
        self.value = 0
        self.avg_value = 0
        self.buffer_full = False
        self.values = []

    def get_value(self) -> float:
        '''returns instantaneous value'''
        with open(self.file_path, 'r', encoding='utf-8') as f:
            self.value = int(f.read().strip()) / 1000000
        return self.value
    
    def get_avg_value(self) -> float:
        '''returns average value'''
        self.values.append(self.get_value())
        if self.buffer_full:
            self.values.pop(0)
        elif len(self.values) >= self.n_sample_avg:
            self.buffer_full = True
        self.avg_value = math.fsum(self.values) / len(self.values)
        return self.avg_value

def get_full_path(base_path, name) -> str:
    '''helper function to find correct hwmon* path for a given device name'''
    for directory in os.listdir(base_path):
        full_path = base_path + directory + '/'
        test_name = open(full_path + "name", encoding="utf8").read().strip()
        if test_name == name:
            return full_path
    print(f"failed to find device {name}")

class FanController():
    '''main FanController class'''
    def __init__(self, debug, config_file):
        self.debug = debug

        # read in config yaml file
        if debug:
            print("reading config file")
        with open(config_file, "r", encoding="utf8") as f:
            try:
                self.config = yaml.safe_load(f)
            except yaml.YAMLError as exc:
                print("error loading config file \n", exc)
                exit(1)

        # store global parameters
        self.base_hwmon_path = self.config["base_hwmon_path"]
        self.loop_interval = self.config["loop_interval"]
        self.control_loop_ratio = self.config["control_loop_ratio"]

        # initialize fan
        fan_path = get_full_path(self.base_hwmon_path, self.config["fan_hwmon_name"])
        self.fan = Fan(fan_path, self.config, self.debug) 

        # initialize list of devices
        self.devices = [ Device(self.base_hwmon_path, device_config, self.fan.max_speed, self.control_loop_ratio, self.debug) for device_config in self.config["devices"] ]

        # initialize APU power sensor
        self.power_sensor = Sensor(self.base_hwmon_path, self.config["sensors"][0], self.debug)

        # exit handler
        signal.signal(signal.SIGTERM, self.on_exit)

    def print_single(self, source_name):
        '''pretty print all device values, temp source, and output'''
        for device in self.devices:
            print(f"{device.nice_name}: {device.temp:.1f}/{device.control_output:.0f}  ", end = '')
            #print("{}: {}  ".format(device.nice_name, device.temp), end = '')
        print(f"{self.power_sensor.nice_name}: {self.power_sensor.value:.1f}/{self.power_sensor.avg_value:.1f}  ", end = '')
        print(f"Fan[{source_name}]: {int(self.fan.fc_speed)}/{self.fan.measured_speed}")

    def loop_read_sensors(self):
        '''internal loop to measure device temps and sensor value'''
        start_time = time.time()
        self.power_sensor.get_avg_value()
        for device in self.devices:
            device.get_avg_temp()
        sleep_time = self.loop_interval - (time.time() - start_time)
        if sleep_time > 0:
            time.sleep(sleep_time)

    def loop_control(self):
        '''main control loop'''
        print("jupiter-fan-control starting up ...")
        while True:
            fan_error = abs(self.fan.fc_speed - self.fan.get_speed())
            if fan_error > 500:
                self.fan.take_control_from_ec()
            # read device temps and power sensor
            for _ in range(self.control_loop_ratio):
                self.loop_read_sensors()
            # read charge state
            self.fan.get_charge_state()
            for device in self.devices:
                device.get_output(device.avg_control_temp, self.power_sensor.avg_value)
            max_output = max(device.control_output for device in self.devices)
            self.fan.set_speed(max_output)
            # find source name for the max control output
            source_name = next(device for device in self.devices if device.control_output == max_output).nice_name
            # print all values
            self.print_single(source_name)

    def on_exit(self, signum, frame):
        '''exit handler'''
        self.fan.return_to_ec_control()
        print("returning fan to EC control loop")
        exit()

# main
if __name__ == '__main__':
    # specify config file path
    CONFIG_FILE_PATH = "/usr/share/jupiter-fan-control/jupiter-fan-control-config.yaml"
    controller = FanController(debug = False, config_file = CONFIG_FILE_PATH)

    args = sys.argv
    if len(args) == 2:
        command = args[1]
        if command == "--run":
            controller.loop_control()
            
    # otherwise, exit cleanly
    controller.on_exit(None, None)
