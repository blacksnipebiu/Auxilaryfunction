namespace Auxilaryfunction;

internal class NumberTranslate
{
    public static string DistanceTranslate(double distance)
    {
        string showdistance;
        if (distance < 1599.5f)
        {
            showdistance = distance.ToString("0") + " m";
        }
        else if (distance < 2400000.0)
        {
            showdistance = (distance / 40000.0).ToString("0.00") + " AU";
        }
        else
        {
            showdistance = (distance / 2400000.0).ToString("0.00") + " " + "光年".Translate();
        }
        return showdistance;
    }
}
