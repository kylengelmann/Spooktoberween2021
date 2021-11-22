using UnityEngine;

public class SpookyUtilities
{
    private readonly static Vector2 NorthVec = Vector2.up;
    private readonly static Vector2 NortheastVec = Vector2.one.normalized;
    private readonly static Vector2 EastVec = Vector2.right;
    private readonly static Vector2 SoutheastVec = new Vector2(1f, -1f).normalized;
    private readonly static Vector2 SouthVec = Vector2.down;
    private readonly static Vector2 SouthwestVec = -NortheastVec;
    private readonly static Vector2 WestVec = Vector2.left;
    private readonly static Vector2 NorthwestVec = -SoutheastVec;

    public static EFaceDirection VectorToFaceDirection(in Vector2 lookVector)
    {
        if(Mathf.Approximately(lookVector.sqrMagnitude, 0f)) return EFaceDirection.None;

        if(lookVector.x >= 0f)
        {
            float EDot = Vector2.Dot(lookVector, EastVec);
            // Q1
            if(lookVector.y > 0f)
            {
                float NDot = Vector2.Dot(lookVector, NorthVec);
                float NEDot = Vector2.Dot(lookVector, NortheastVec);

                if(NEDot > NDot && NEDot > EDot)
                {
                    return EFaceDirection.Northeast;
                }
                else if(EDot > NDot)
                {
                    return EFaceDirection.East;
                }
                else return EFaceDirection.North;
            }
            // Q4
            else
            {
                float SDot = Vector2.Dot(lookVector, SouthVec);
                float SEDot = Vector2.Dot(lookVector, SoutheastVec);

                if (SEDot > SDot && SEDot > EDot)
                {
                    return EFaceDirection.Southeast;
                }
                else if (EDot > SDot)
                {
                    return EFaceDirection.East;
                }
                else return EFaceDirection.South;
            }
        }
        else
        {
            float WDot = Vector2.Dot(lookVector, WestVec);
            // Q2
            if (lookVector.y > 0f)
            {
                float NDot = Vector2.Dot(lookVector, NorthVec);
                float NWDot = Vector2.Dot(lookVector, NorthwestVec);

                if (NWDot > NDot && NWDot > WDot)
                {
                    return EFaceDirection.Northwest;
                }
                else if (WDot > NDot)
                {
                    return EFaceDirection.West;
                }
                else return EFaceDirection.North;
            }
            // Q3
            else
            {
                float SDot = Vector2.Dot(lookVector, SouthVec);
                float SWDot = Vector2.Dot(lookVector, SouthwestVec);

                if (SWDot > SDot && SWDot > WDot)
                {
                    return EFaceDirection.Southwest;
                }
                else if (WDot > SDot)
                {
                    return EFaceDirection.West;
                }
                else return EFaceDirection.South;
            }
        }
    }
}
