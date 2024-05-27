using System;
using System.Collections.Generic;
using System.Linq;

using EqLib;

namespace CalLib
{
    public class NaSetup
    {
        public static Dictionary<int, NaSetup> Chan = new Dictionary<int, NaSetup>();

        public List<EqSwitchMatrix.PortCombo> NaPortCombos = new List<EqSwitchMatrix.PortCombo>();
        public static Dictionary<EqSwitchMatrix.InstrPort, int> SwitchMatrixConnections = new Dictionary<EqSwitchMatrix.InstrPort, int>();
        public List<int> NaPorts = new List<int>();
        public string NaPortsString = "";
        public string DutPortsString = "";
        public static Dictionary<string, CalStd> CalStds = new Dictionary<string, CalStd>();

        public static void DefineSwitchMatrixConnection(EqSwitchMatrix.InstrPort instrPort, int naPort)
        {
            SwitchMatrixConnections[instrPort] = naPort;
        }

        public static void DefineChannel(int NaChanNum, EqSwitchMatrix.PortCombo PortCombo1 = null, EqSwitchMatrix.PortCombo PortCombo2 = null, EqSwitchMatrix.PortCombo PortCombo3 = null, EqSwitchMatrix.PortCombo PortCombo4 = null)
        {
            Chan[NaChanNum] = new NaSetup(); ;

            if (PortCombo1 != null) Chan[NaChanNum].NaPortCombos.Add(PortCombo1);
            if (PortCombo2 != null) Chan[NaChanNum].NaPortCombos.Add(PortCombo2);
            if (PortCombo3 != null) Chan[NaChanNum].NaPortCombos.Add(PortCombo3);
            if (PortCombo4 != null) Chan[NaChanNum].NaPortCombos.Add(PortCombo4);

            Chan[NaChanNum].NaPorts = Chan[NaChanNum].NaPortCombos.Select(x => SwitchMatrixConnections[x.instrPort]).OrderBy(x => x).ToList();
            Chan[NaChanNum].NaPortsString = string.Join(",", Chan[NaChanNum].NaPorts);
            Chan[NaChanNum].DutPortsString = string.Join(", ", Chan[NaChanNum].NaPortCombos.Select(x => x.dutPort).OrderBy(x => x));
        }

        public static void DefineChannel(int NaChanNum, string band, Operation op1 = Operation.MeasureH3_PS, Operation op2 = Operation.MeasureH3_PS, Operation op3 = Operation.MeasureH3_PS, Operation op4 = Operation.MeasureH3_PS)
        {
            //EqSwitchMatrix.PortCombo PortCombo1 = op1 != Operation.MeasureH3 ? SwitchMatrix.Maps.Get(band, op1) : null;
            //EqSwitchMatrix.PortCombo PortCombo2 = op2 != Operation.MeasureH3 ? SwitchMatrix.Maps.Get(band, op2) : null;
            //EqSwitchMatrix.PortCombo PortCombo3 = op3 != Operation.MeasureH3 ? SwitchMatrix.Maps.Get(band, op3) : null;
            //EqSwitchMatrix.PortCombo PortCombo4 = op4 != Operation.MeasureH3 ? SwitchMatrix.Maps.Get(band, op4) : null;

            //DefineChannel(NaChanNum, PortCombo1, PortCombo2, PortCombo3, PortCombo4);
        }

        public static string GetTrayCoordinates(CalStd.Type calStdType)
        {
            foreach (CalStd calStd in CalStds.Values)
            {
                if (calStd.StdType == calStdType)
                {
                    return calStd.xyTrayCoord;
                }
            }

            throw new Exception("Cal Standard not defined for " + calStdType);
        }

        public static string GetTrayCoordinates(CalStd.Type calStdType, string band, Operation op1, Operation op2)
        {
            return GetTrayCoordinates(calStdType, Eq.Site[0].SwMatrix.GetPath(band, op1), Eq.Site[0].SwMatrix.GetPath(band, op2));
        }

        public static string GetTrayCoordinates(CalStd.Type calStdType, EqSwitchMatrix.PortCombo PortComboA, EqSwitchMatrix.PortCombo PortComboB)
        {
            CalStd.ThruCombo tempThru = new CalStd.ThruCombo(PortComboA, PortComboB);

            foreach (CalStd calStd in CalStds.Values)
            {
                if (calStd.StdType == calStdType && calStd.Thrus != null)
                {
                    foreach (CalStd.ThruCombo thru in calStd.Thrus)
                    {
                        if (thru == tempThru)
                        {
                            return calStd.xyTrayCoord;
                        }
                    }
                }
            }

            throw new Exception("Cal Standard not defined for " + calStdType);
        }

        public class CalStd
        {
            public string ID;
            public string xyTrayCoord;
            public Type StdType;

            public List<ThruCombo> Thrus;

            public enum Type
            {
                Short, Open, Load, Thru
            }

            public class ThruCombo
            {
                public EqSwitchMatrix.PortCombo PortComboA, PortComboB;

                public ThruCombo(EqSwitchMatrix.PortCombo PortComboA, EqSwitchMatrix.PortCombo PortComboB)
                {
                    if (SwitchMatrixConnections[PortComboA.instrPort] < SwitchMatrixConnections[PortComboB.instrPort])
                    {
                        this.PortComboA = PortComboA;
                        this.PortComboB = PortComboB;
                    }
                    else
                    {
                        this.PortComboA = PortComboB;
                        this.PortComboB = PortComboA;
                    }
                }

                public override bool Equals(Object obj)
                {
                    if (obj == null || GetType() != obj.GetType())
                        return false;

                    ThruCombo p = (ThruCombo)obj;
                    return (PortComboA == p.PortComboA) && (PortComboB == p.PortComboB);
                }
                public override int GetHashCode()
                {
                    unchecked
                    {
                        int hash = 17;
                        hash = hash * 29 + PortComboA.GetHashCode();
                        hash = hash * 29 + PortComboB.GetHashCode();
                        return hash;
                    }
                }
                public static bool operator ==(ThruCombo x, ThruCombo y)
                {
                    return Object.Equals(x, y);
                }
                public static bool operator !=(ThruCombo x, ThruCombo y)
                {
                    return !(x == y);
                }
            }

            public CalStd(string ID, string xyTrayCoord, Type StdType)
            {
                this.ID = ID;
                this.xyTrayCoord = xyTrayCoord;
                this.StdType = StdType;

                Thrus = null;
            }

            public static void Define(string CalStdID, string xyTrayCoord, Type calStandard)
            {
                CalStds.Add(CalStdID, new CalStd(CalStdID, xyTrayCoord, calStandard));
            }

            public static void AddThru(string CalStdID, string band, Operation op1, Operation op2)
            {
                //if (CalStds[CalStdID].Thrus == null) CalStds[CalStdID].Thrus = new List<ThruCombo>();

                //CalStds[CalStdID].Thrus.Add(new CalStd.ThruCombo(
                //        SwitchMatrix.Maps.Get(band, op1),
                //        SwitchMatrix.Maps.Get(band, op2)));
            }

        }
    }
}
