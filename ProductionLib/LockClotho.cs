using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security;
using Accessibility;

namespace ProductionLib2
{
    #region Clotho Input Lock

    public static class LockClotho
    {
        private static IntPtr GetClothoHandler(string title)
        {
            IntPtr hwndWindow = IntPtr.Zero;

            IEnumerable<IntPtr> hwnds = UnsafeNativeMethods.FindWindowsWithText(title);

            foreach (IntPtr hwnd in hwnds)
            {
                hwndWindow = hwnd;
                break;
            }
            return hwndWindow;
        }

        private static void EnableClothoTextBoxes(bool enabled, string winform_title)
        {
            IntPtr hwndWindow = GetClothoHandler(winform_title);

            if (hwndWindow != IntPtr.Zero)
            {
                List<IntPtr> hwndControls = UnsafeNativeMethods.GetChildWindows(hwndWindow);
                StringBuilder className = new StringBuilder(256);

                foreach (IntPtr hwndControl in hwndControls)
                {
                    UnsafeNativeMethods.GetClassName(hwndControl, className, className.Capacity);

                    if (className.ToString().Contains("WindowsForms10.EDIT."))
                    {
                        UnsafeNativeMethods.EnableWindow(hwndControl, (uint)(enabled == true ? 1 : 0)); // 0 for false, 1 for true
                    }
                }
            }
        }

        private static void EnableClothoStartButton(bool enabled, string winform_title)
        {
            IntPtr hwndWindow = GetClothoHandler(winform_title);

            if (hwndWindow != IntPtr.Zero)
            {
                List<IntPtr> hwndControls = UnsafeNativeMethods.GetChildWindows(hwndWindow);
                StringBuilder className = new StringBuilder(256);

                foreach (IntPtr hwndControl in hwndControls)
                {
                    UnsafeNativeMethods.GetClassName(hwndControl, className, className.Capacity);

                    if (className.ToString().Contains("WindowsForms10.BUTTON."))
                    {
                        StringBuilder buttonText = new StringBuilder(256);

                        UnsafeNativeMethods.GetWindowText(hwndControl, buttonText, buttonText.Capacity);

                        if (buttonText.ToString() == "START")
                        {
                            UnsafeNativeMethods.EnableWindow(hwndControl, (uint)(enabled == true ? 1 : 0)); // 0 for false, 1 for true
                        }
                    }
                }
            }
        }

        public static bool IsProductionLoadedMode(string winform_title)
        {
            bool bProduction = false;

            IntPtr hwndWindow = GetClothoHandler(winform_title);

            if (hwndWindow != IntPtr.Zero)
            {
                List<IntPtr> hwndControls = UnsafeNativeMethods.GetChildWindows(hwndWindow);
                StringBuilder className = new StringBuilder(256);

                foreach (IntPtr hwndControl in hwndControls)
                {
                    UnsafeNativeMethods.GetClassName(hwndControl, className, className.Capacity);

                    if (className.ToString().Contains("WindowsForms10.BUTTON."))
                    {
                        StringBuilder buttonText = new StringBuilder(256);

                        UnsafeNativeMethods.GetWindowText(hwndControl, buttonText, buttonText.Capacity);

                        if (buttonText.ToString() == "START")
                        {
                            bProduction = !UnsafeNativeMethods.IsWindowEnabled(hwndControl);
                        }
                    }
                }
            }

            return bProduction;
        }

        public static void LockClothoInputUI(object winform_title)
        {
            string title = (string)winform_title;

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(200);
                LockClotho.EnableClothoTextBoxes(false, title);
            }
        }

        public static void UnlockClothoInputUI(object winform_title)
        {
            LockClotho.EnableClothoTextBoxes(true, (string)winform_title);
        }
    }

    public delegate bool CallBack(int hwnd, int lParam);

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowEnabled(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int EnableWindow(IntPtr hWnd, uint bEnable);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int EnumWindows(CallBack x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetNextDlgTabItem(IntPtr hDlg, IntPtr hCtl, int bPrevious);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetNextDlgGroupItem(IntPtr hDlg, IntPtr hCtl, int bPrevious);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(HandleRef hwnd, uint wMsg, IntPtr wParam, string lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern IntPtr SendMessageInt(HandleRef hwnd, uint wMsg, IntPtr wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary> Get the text for the window pointed to by hWnd </summary>
        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);

        /// <summary>
        /// Returns a list of child windows
        /// </summary>
        /// <param name="parent">Parent of the windows to return</param>
        /// <returns>List of child windows</returns>
        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        /// <summary>
        /// Callback method to be used when enumerating windows.
        /// </summary>
        /// <param name="handle">Handle of the next window</param>
        /// <param name="pointer">Pointer to a GCHandle that holds a reference to the list to fill</param>
        /// <returns>True to continue the enumeration, false to bail</returns>
        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        /// <summary> Find all windows that match the given filter </summary>
        /// <param name="filter"> A delegate that returns true for windows
        ///    that should be returned and false for windows that should
        ///    not be returned </param>
        public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    // only add the windows that pass the filter
                    windows.Add(wnd);
                }

                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary> Find all windows that contain the given title text </summary>
        /// <param name="titleText"> The text that the window title must contain. </param>
        public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows(delegate (IntPtr wnd, IntPtr param)
            {
                return GetWindowText(wnd).Contains(titleText);
            });
        }

        /// <summary>
        /// Delegate for the EnumChildWindows method
        /// </summary>
        /// <param name="hWnd">Window handle</param>
        /// <param name="parameter">Caller-defined variable; we use it for a pointer to our list</param>
        /// <returns>True to continue enumerating, false to bail.</returns>
        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        public const UInt32 SW_HIDE = 0;
        public const UInt32 SW_SHOWNORMAL = 1;
        public const UInt32 SW_NORMAL = 1;
        public const UInt32 SW_SHOWMINIMIZED = 2;
        public const UInt32 SW_SHOWMAXIMIZED = 3;
        public const UInt32 SW_MAXIMIZE = 3;
        public const UInt32 SW_SHOWNOACTIVATE = 4;
        public const UInt32 SW_SHOW = 5;
        public const UInt32 SW_MINIMIZE = 6;
        public const UInt32 SW_SHOWMINNOACTIVE = 7;
        public const UInt32 SW_SHOWNA = 8;
        public const UInt32 SW_RESTORE = 9;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hwnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPlacement(IntPtr hwnd, ref WINDOWPLACEMENT lpwndpl);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int L;
            public int T;
            public int R;
            public int B;
        };
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(int hwnd, out RECT lpRect);

        [DllImport("Oleacc.dll")]
        public static extern int AccessibleObjectFromWindow(
        IntPtr hwnd,
        int dwObjectID,
        ref Guid refID,
        ref IAccessible ppvObject);

        [DllImport("Oleacc.dll")]
        public static extern int WindowFromAccessibleObject(IAccessible pacc, out IntPtr phwnd);

        [DllImport("Oleacc.dll")]
        public static extern int AccessibleChildren(Accessibility.IAccessible paccContainer, int iChildStart, int cChildren, [Out] object[] rgvarChildren, out int pcObtained);

        public const uint WM_SETTEXT = 0x000C;
        public const uint WM_CLICK = 0x00F5;

        public const int CHILDID_SELF = 0;
        public const int CHILDID_1 = 1;
        public const int OBJID_CLIENT = -4;
    }

    #endregion Clotho Input Lock
}
