using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PerformanceOverlay
{
    internal class Overlays
    {
        public class Entry
        {
            public String? Text { get; set; }
            public IList<OverlayMode> Include { get; set; } = new List<OverlayMode>();
            public IList<OverlayMode> Exclude { get; set; } = new List<OverlayMode>();
            public IList<Entry> Nested { get; set; } = new List<Entry>();
            public String Separator { get; set; } = "";
            public bool IgnoreMissing { get; set; }

            public static readonly Regex attributeRegex = new Regex("{([^}]+)}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            public Entry()
            { }

            public Entry(String text)
            {
                this.Text = text;
            }

            public IEnumerable<Match> AllAttributes
            {
                get
                {
                    return attributeRegex.Matches(Text ?? "");
                }
            }

            private String EvaluateText(Sensors sensors)
            {
                String output = Text ?? "";

                foreach (var attribute in AllAttributes)
                {
                    String attributeName = attribute.Groups[1].Value;
                    String? value = sensors.GetValue(attributeName);
                    if (value is null && IgnoreMissing)
                        return "";
                    output = output.Replace(attribute.Value, value ?? "-");
                }

                return output;
            }

            public String? GetValue(OverlayMode mode, Sensors sensors)
            {
                if (Exclude.Count > 0 && Exclude.Contains(mode))
                    return null;
                if (Include.Count > 0 && !Include.Contains(mode))
                    return null;

                String output = EvaluateText(sensors);

                if (Nested.Count > 0)
                {
                    var outputs = Nested.Select(entry => entry.GetValue(mode, sensors)).Where(output => output != null);
                    if (outputs.Count() == 0)
                        return null;

                    output += String.Join(Separator, outputs);
                }

                if (output == String.Empty)
                    return null;

                return output;
            }
        }

        public static readonly String[] Helpers =
        {
            "<C0=008040><C1=0080C0><C2=C08080><C3=FF0000><C4=FFFFFF><C250=FF8000>",
            "<A0=-4><A1=5><A2=-2><A3=-3><A4=-4><A5=-5><S0=-50><S1=50>",
        };

        public static readonly Entry OSD = new Entry
        {
            Nested = {
                // Simple just FPS
                new Entry {
                    Nested =
                    {
                        new Entry("<C4><FR><C><A><A1><S1><C4> FPS<C><S><A>"),
                        new Entry("<C4><A3>{BATT_%}<A><A1><S1> %<C><S><A>") { Include = { OverlayMode.FPSWithBattery }, IgnoreMissing = true },
                        new Entry("<C4><A4>{BATT_W}<A><A1><S1> W<C><S><A>") { Include = { OverlayMode.FPSWithBattery }, IgnoreMissing = true },
                        new Entry("<C4><A3>{BATT_MIN}<A><A1><S1> min<C><S><A>") { Include = { OverlayMode.FPSWithBattery }, IgnoreMissing = true }
                    },
                    Include = { OverlayMode.FPS, OverlayMode.FPSWithBattery }
                },

                // Minimal and Detail
                new Entry {
                    Nested =
                    {
                        new Entry
                        {
                            Text = "<C1>BAT<C>",
                            Nested =
                            {
                                new Entry("<C4><A3>{BATT_%}<A><A1><S1> %<S><A>"),
                                new Entry("<C4><A4>{BATT_W}<A><A1><S1> W<S><A>") { IgnoreMissing = true },
                                new Entry("<C4><A3>{BATT_MIN}<A><A1><S1> min<S><A>") { IgnoreMissing = true },
                                new Entry("C<C4><A4>{BATT_CHARGE_W}<A><A1><S1> W<S><A>") { IgnoreMissing = true, Include = { OverlayMode.Detail } }
                            }
                        },
                        new Entry
                        {
                            Text = "<C1>GPU<C>",
                            Nested =
                            {
                                new Entry("<C4><A3>{GPU_%}<A><A1><S1> %<S><A>"),
                                new Entry("<C4><A4>{GPU_W}<A><A1><S1> W<S><A>"),
                                new Entry("<C4><A4>{GPU_T}<A><A1><S1> C<S><A>") { IgnoreMissing = true, Include = { OverlayMode.Detail } }
                            }
                        },
                        new Entry
                        {
                            Text = "<C1>CPU<C>",
                            Nested =
                            {
                                new Entry("<C4><A3>{CPU_%}<A><A1><S1> %<S><A>"),
                                new Entry("<C4><A4>{CPU_W}<A><A1><S1> W<S><A>"),
                                new Entry("<C4><A4>{CPU_T}<A><A1><S1> C<S><A>") { IgnoreMissing = true, Include = { OverlayMode.Detail } }
                            }
                        },
                        new Entry
                        {
                            Text = "<C1>RAM<C>",
                            Nested = { new Entry("<C4><A5>{MEM_GB}<A><A1><S1> GiB<S><A>") }
                        },
                        new Entry
                        {
                            Text = "<C1>FAN<C>",
                            Nested = { new Entry("<C4><A5>{FAN_RPM}<A><A1><S1> RPM<S><A>") },
                            Include = { OverlayMode.Detail }
                        },
                        new Entry
                        {
                            Text = "<C2><APP><C>",
                            Nested = { new Entry("<C4><A4><FR><C><A><A1><S1><C4> FPS<C><S><A>") }
                        },
                        new Entry
                        {
                            Text = "<C2>[OBJ_FT_SMALL]<C><S1> <C4><A0><FT><A><A1> ms<A><S><C>",
                            Include = { OverlayMode.Detail }
                        }
                    },
                    Separator = "<C250>|<C> ",
                    Include = { OverlayMode.Minimal, OverlayMode.Detail }
                },

                new Entry {
                    Nested =
                    {
                        new Entry("<C1>CPU<C>\t  ")
                        {
                            Nested = {
                                new Entry("<A5>{CPU_%}<A><A1><S1> %<S><A>"),
                                new Entry("<A5>{CPU_W}<A><A1><S1> W<S>"),
                                new Entry("<A5>{CPU_T}<A><A1><S1> C<S><A>") { IgnoreMissing = true },
                            }
                        },
                        new Entry("\t  ")
                        {
                            Nested = {
                                new Entry("<A5>{MEM_MB}<A><A1><S1> MB<S>"),
                                new Entry("<A5>{CPU_MHZ}<A><A1><S1> MHz<S><A>")
                            }
                        },
                        new Entry("<C1>GPU<C>\t  ")
                        {
                            Nested = {
                                new Entry("<A5>{GPU_%}<A><A1><S1> %<S><A>"),
                                new Entry("<A5>{GPU_W}<A><A1><S1> W<S><A>"),
                                new Entry("<A5>{GPU_T}<A><A1><S1> C<S><A>") { IgnoreMissing = true },
                            }
                        },
                        new Entry("\t  ")
                        {
                            Nested = {
                                new Entry("<A5>{GPU_MB}<A><A1><S1> MB<S><A>"),
                                new Entry("<A5>{GPU_MHZ}<A><A1><S1> MHz<S><A>") { IgnoreMissing = true }
                            }
                        },
                        new Entry("<C1>FAN<C>\t  ")
                        {
                            Nested = {
                                new Entry("<A5>{FAN_RPM}<A><A1><S1> RPM<S><A>"),
                            }
                        },
                        new Entry("<C2><APP><C>\t  ")
                        {
                            Nested = {
                                new Entry("<A5><C4><FR><C><A><A1><S1><C4> FPS<C><S><A>"),
                                new Entry("<A5><C4><FT><C><A><A1><S1><C4> ms<C><S><A>"),
                            }
                        },
                        new Entry("<C1>BAT<C>\t  ") {
                            Nested = {
                                new Entry("<A5>{BATT_%}<A><A1><S1> %<S><A>"),
                                new Entry("<A5>{BATT_W}<A><A1><S1> W<S><A>") { IgnoreMissing = true },
                                new Entry("<A5>{BATT_MIN}<A><A1><S1> min<S><A>") { IgnoreMissing = true },
                                new Entry("<A5>C{BATT_CHARGE_W}<A><A1><S1> W<S><A>") { IgnoreMissing = true }
                            }
                        },
                        new Entry("<C2><S1>Frametime<S>"),
                        new Entry("[OBJ_FT_LARGE]<S1> <A0><FT><A><A1> ms<A><S><C>"),
                    },
                    Separator = "\r\n",
                    Include = { OverlayMode.Full }
                }
            }
        };

        public static String GetOSD(OverlayMode mode, Sensors sensors)
        {
            var sb = new StringBuilder();

            sb.AppendJoin("", Helpers);
            sb.Append(OSD.GetValue(mode, sensors) ?? "");

            return sb.ToString();
        }
    }
}
