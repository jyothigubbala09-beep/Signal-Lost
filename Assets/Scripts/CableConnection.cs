public enum CableType
{
    Straight,
    Corner,
    TJunction,
    Cross,
    EndPoint
}

public static class CableConnection
{
    // Clockwise direction indices: 0 = Up, 1 = Right, 2 = Down, 3 = Left
    public const int UP = 0;
    public const int RIGHT = 1;
    public const int DOWN = 2;
    public const int LEFT = 3;

    public static int GetOpposite(int direction)
    {
        return (direction + 2) % 4;
    }

    public static bool[] GetBaseConnections(CableType type)
    {
        bool[] conns = new bool[4];
        switch (type)
        {
            case CableType.Straight:
                conns[UP] = true;
                conns[DOWN] = true;
                break;
            case CableType.Corner:
                conns[UP] = true;
                conns[RIGHT] = true;
                break;
            case CableType.TJunction:
                conns[UP] = true;
                conns[RIGHT] = true;
                conns[DOWN] = true;
                break;
            case CableType.Cross:
                conns[UP] = true;
                conns[RIGHT] = true;
                conns[DOWN] = true;
                conns[LEFT] = true;
                break;
            case CableType.EndPoint:
                conns[UP] = true;
                break;
        }
        return conns;
    }
}
