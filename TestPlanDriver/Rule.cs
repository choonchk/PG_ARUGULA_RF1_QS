
#region COMMNET and Copyright SECTION (NO MANUAL TOUCH!)
// This is AUTOMATIC generated template Adaptive Sampling Rule (.cs) file for ATF (Clotho) of WSD, AvagoTech: V2.2.1.0
// Any Questions or Comments, Please Contact: YU HAN, yu.han@avagotech.com
// NOTE: Clotho Adaptive Sampling Rule have 'FIXED' Sections which should NEVER be Manually Touched 
#endregion COMMNET and Copyright SECTION

#region 'FIXED' Reference Section (NO MANUAL TOUCH!)
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;
using Avago.ATF.Validators;
#endregion 'FIXED' Reference Section


#region Custom Reference Section
//////////////////////////////////////////////////////////////////////////////////
// ----------- ONLY provide your Custom Reference 'Usings' here --------------- //


// ----------- END of Custom Reference 'Usings' --------------- //
//////////////////////////////////////////////////////////////////////////////////
#endregion Custom Reference Section


public class RuleDummyAlwaysTestAll3Paras : MarshalByRefObject, IATFAdaptiveSampling
{

    /// <summary>
    /// Use previous bitmask, plus current run's result, to generate next run's bitmask
    ///     NOTE there's no parameters, all required information are transferred through cross-domain 
    /// </summary>
    /// <param name="inputparas">Reserved for possible future usage. For now just ignore</param>
    /// <returns>If algo succeed, return "; otherwise, return string with "RuleRunFailure: "as starting</returns>
    public string AdjustBitMask(string inputparas)
    {
        #region DO NOT MANUAL TOUCH

        Dictionary<string, bool> currentASRuleBitMask = ATFCrossDomainWrapper.GetASRuleBitMask();
        Dictionary<string, List<double>> historyResult = ATFCrossDomainWrapper.GetHistoryDUTRunResultSet();

        #endregion

        #region Start of your custom Rule Algorithm

        // The example Adaptive Sampling Algorithm section. 
        // I just make a simplest case, use the last 3 (if available) runs's run result's average value to decide the new Mask 

        // Always true for #1 
        currentASRuleBitMask["Para1"] = true;

        int completedDUTsCount = historyResult["Para1"].Count;

        // NOTE here, since this algorithm always test "Para1", so no need to check whether the value is NaN, 
        // Otherwise you need consider possible value is NaN case 

        double para1Last3RunsAvgValue = 0;
        if (completedDUTsCount == 1)
            para1Last3RunsAvgValue = historyResult["Para1"][completedDUTsCount - 1];
        else if (completedDUTsCount == 2)
            para1Last3RunsAvgValue = (historyResult["Para1"][completedDUTsCount - 1] + historyResult["Para1"][completedDUTsCount - 2]) / 2;
        else
            para1Last3RunsAvgValue = (historyResult["Para1"][completedDUTsCount - 1] + historyResult["Para1"][completedDUTsCount - 2] + historyResult["Para1"][completedDUTsCount - 3]) / 3;

        // Use #1 value to decide #2 and #3 skip or not 
        if ((para1Last3RunsAvgValue < 1) || (para1Last3RunsAvgValue > 5))
        {
            // #1 not qualified, so let's test #2 and #3
            currentASRuleBitMask["Para2"] = true;
            currentASRuleBitMask["Para3"] = true;
        }
        else
        {
            // Otherwise, for next run, let's skip #2 and #3
            currentASRuleBitMask["Para2"] = false;
            currentASRuleBitMask["Para3"] = false;
        }

        #endregion

        #region DO NOT MANUAL TOUCH

        // All succeed, now update back to CD
        ATFCrossDomainWrapper.UpdateASRuleBitMask(currentASRuleBitMask);
        return "";

        #endregion
    }
}
