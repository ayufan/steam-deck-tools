// If L5 is hold, the A, B, X, Y is turbo: 10x per second
if (Steam.BtnL5)
{
    X360.BtnA = Turbo(Steam.BtnA, 10);
    X360.BtnB = Turbo(Steam.BtnB, 10);
    X360.BtnX = Turbo(Steam.BtnX, 10);
    X360.BtnY = Turbo(Steam.BtnY, 10);
}

