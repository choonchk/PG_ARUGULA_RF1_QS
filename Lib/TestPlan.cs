﻿#region COMMNET and Copyright SECTION (NO MANUAL TOUCH!)
// This is AUTOMATIC generated template Test Plan (.cs) file for ATF (Clotho) of WSD, AvagoTech: V2.2.1.0
// Any Questions or Comments, Please Contact: YU HAN, yu.han@avagotech.com
// NOTE 1: Test Plan template .cs has 'FIXED' Sections which should NEVER be Manually Touched 
// NOTE 2: Starting from V2.2.0, Clotho follows new Package style test plan management:
//       (a) Requires valid integer Version defined for TestPlan, TestLimit, and ExcelUI
//               For TestPlan.cs, refer to header item 'TestPlanVersion=1'
//               For TestLiimit.csv, refer to row #7 'SpecVersion,1'
//               For ExcelUI.xlsx, refer to sheet #1, row #1 'VER	1'
//       Note TestPlanTemplateGenerator generated items holds default version as '1'
//       (b) About ExcelUI file and TestLimit file:
//               Always load from same parent folder as Test Plan .cs, @ root level
//       (c) About Correlation File:
//               When Development mode, loaded from  C:\Avago.ATF.Common.x64\CorrelationFiles\Development\
//               When Production mode, loaded from package folder within C:\Avago.ATF.Common.x64\CorrelationFiles\Production\
#endregion COMMNET and Copyright SECTION

#region Test Plan Properties Section (NO MANUAL TOUCH)
////<TestPlanVersion>TestPlanVersion=1<TestPlanVersion/>
////<ExcelBuddyConfig>BuddyExcel = ENGR-8255-AP1-QA-RF1_B1A_EVT_TCF_v0001.xlsx;ExcelDisplay = 1<ExcelBuddyConfig/>
////<TestLimitBuddyConfig>BuddyTestLimit = ENGR-8255-AP1-QA-RF1_B1A_EVT_TSF_v0001.csv<TestLimitBuddyConfig/>
////<CorrelationBuddyConfig>BuddyCorrelaton = ENGR-8255-AP1-RF1_B1A_EVT_CF_v0001.csv<CorrelationBuddyConfig/>
#endregion Test Plan Properties Section

#region Test Plan Hardware Configuration Section
//<HWConfig>AliasName="NA02";EquipmentName="NA02";Address="NA02"<HWConfig/>
#endregion Test Plan Hardware Configuration Section

#region Test Plan Parameters Section
////<TestParameter>Name="SimHW";Type="IntType";Unit=""<TestParameter/>
#endregion Test Plan Parameters Section

#region Singel Value Parameters Section
////<SingelValueParameter>Name="SimHW";Value="1";Type="IntType";Unit=""<SingelValueParameter/>
#endregion Singel Value Parameters Section

#region Test Plan Sweep Control Section (NO MANUAL TOUCH!)
#endregion Test Plan Sweep Control Section

#region 'FIXED' Reference Section (NO MANUAL TOUCH!)
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualBasic;

using Avago.ATF.StandardLibrary;
using Avago.ATF.Shares;

#endregion 'FIXED' Reference Section

#region Custom Reference Section
//////////////////////////////////////////////////////////////////////////////////
// ----------- ONLY provide your Custom Reference 'Usings' here --------------- //
using TestPlanCommon;
using TestPlanCommon.CommonModel;
using TestPlan_NUWAFull;
// ----------- END of Custom Reference 'Usings' --------------- //
//////////////////////////////////////////////////////////////////////////////////
#endregion Custom Reference Section


public class TestPlan : MarshalByRefObject, IATFTest
{
    //private IATFTest testPlan = new TestPlan_NightHawkFull.NightHawkFbar();
    private IATFTest testPlan = new TestPlan_NUWAFull.NUWAFull(); // need to automation to select tester type
    
    public string DoATFInit(string args)
    {
        //Debugger.Break();
        testPlan.DoATFInit(args);

        return string.Concat("Enter DoATFInit: {0}\nDo Minimum HW Init:\n{1}\n", args, ATFInitializer.DoMinimumHWInit());
    }

    public string DoATFLot(string args)
    {
        //Debugger.Break();

        testPlan.DoATFLot(args);

        return string.Concat("Enter DoATFLot: {0}\n", args);
    }

    public ATFReturnResult DoATFTest(string args)
    {
        //Debugger.Break();

        return testPlan.DoATFTest(args);
    }

    public string DoATFUnInit(string args)
    {
        //Debugger.Break();
        if (testPlan==null) return string.Concat("Enter DoATFUnInit: {0}\n", args);
        testPlan.DoATFUnInit(args);

        return string.Concat("Enter DoATFUnInit: {0}\n", args);
    }
}