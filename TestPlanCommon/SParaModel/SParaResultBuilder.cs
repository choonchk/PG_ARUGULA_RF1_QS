using System.Collections.Generic;
using Avago.ATF.StandardLibrary;
using LibFBAR_TOPAZ.DataType;
using ResultBuilder = TestLib.ResultBuilder;

namespace TestPlanCommon.SParaModel
{
    public class SParaResultBuilder
    {
        public static void BuildResult(List<s_Result> libfbarResult, ATFReturnResult results)
        {
            ResultDataCollection rdc = BuildResult2(libfbarResult);
            rdc.Add(results);
        }

        private static List<s_Result> GetResultList(List<s_Result> libfbarResult)
        {
            List<s_Result> rl = new List<s_Result>();
            foreach (s_Result r in libfbarResult)
            {
                bool isValid = r.Enable && !string.IsNullOrEmpty(r.Result_Header);
                if (!isValid) continue;
                rl.Add(r);
            }

            return rl;
        }

        private static List<s_Result> GetDcResultList(List<s_Result> libfbarResult)
        {
            List<s_Result> rl = new List<s_Result>();
            foreach (s_Result r in libfbarResult)
            {
                bool isMultiResult = r.IsHasMultiResult;
                if (!isMultiResult) continue;
                rl.Add(r);
            }

            return rl;
        }

        private static ResultDataCollection BuildResult2(List<s_Result> libfbarResult)
        {
            List<s_Result> cleanRlList = GetResultList(libfbarResult);
            ResultDataCollection rdc = new ResultDataCollection();

            foreach (s_Result r in cleanRlList)
            {
                #region Determine Parameter Units
                string rHeader = r.Result_Header.ToUpper();
                var param_units = "";
                if (rHeader.Contains("GD") || rHeader.Contains("GDEL"))
                {
                    param_units = "sec";
                }
                else if (rHeader.Contains("PHASE"))
                {
                    param_units = "deg";
                }
                else
                {
                    param_units = "dB";
                }
                #endregion Determine Parameter Units
                rdc.Add(r, param_units);
                // DC result reported last.

            }

            cleanRlList = GetDcResultList(libfbarResult);
            foreach (s_Result r in cleanRlList)
            {
                List<s_mRslt> dcResultList = r.GetMultiResultList();
                foreach (s_mRslt dcResult in dcResultList)
                {
                    rdc.Add(dcResult, "A");
                }

            }

            return rdc;
        }
    }

    public class ResultDataCollection
    {
        public List<ResultDataObject> List { get; }

        public ResultDataCollection()
        {
            List = new List<ResultDataObject>();
        }

        public void Add(ATFReturnResult atfrR)
        {
            foreach (ResultDataObject rDo in List)
            {
                // Add to ResultBuilder and ATFResultBuilder (can improve).
                ATFResultBuilder.AddResult(ref atfrR, rDo.Name, rDo.Unit, rDo.Value);
                ResultBuilder.AddResult(0, rDo.Name, rDo.Unit, rDo.Value);
            }
        }

        public void Add(s_mRslt result, string unit)
        {
            ResultDataObject rDo = new ResultDataObject();
            rDo.Name = result.Result_Header;
            rDo.Unit = unit;
            rDo.Value = (float)result.Result_Data;
            List.Add(rDo);
        }

        public void Add(s_Result result, string unit)
        {
            ResultDataObject rDo = new ResultDataObject();
            rDo.Name = result.Result_Header;
            rDo.Unit = unit;
            rDo.Value = (float)result.Result_Data;
            List.Add(rDo);
        }

        public void Add(string name, string unit, double value)
        {
            ResultDataObject rDo = new ResultDataObject();
            rDo.Name = name;
            rDo.Unit = unit;
            rDo.Value = (float)value;
            List.Add(rDo);
        }

        public void Add(ResultDataCollection rdCollection)
        {
            List.AddRange(rdCollection.List);
        }
    }

    public class ResultDataObject
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public double Value { get; set; }
    }
}