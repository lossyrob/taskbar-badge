using System.Runtime.InteropServices;
using System.Windows;
using TaskbarBadge.Core;

namespace TaskbarBadge.App;

public sealed class TaskbarInfoProvider
{
    private const uint AbmGetTaskbarPos = 0x00000005;

    public TaskbarSnapshot GetPrimaryTaskbarSnapshot()
    {
        var appBarData = new AppBarData
        {
            cbSize = Marshal.SizeOf<AppBarData>()
        };

        var result = SHAppBarMessage(AbmGetTaskbarPos, ref appBarData);
        if (result != IntPtr.Zero)
        {
            var bounds = new DesktopRect(
                appBarData.rc.Left,
                appBarData.rc.Top,
                appBarData.rc.Right - appBarData.rc.Left,
                appBarData.rc.Bottom - appBarData.rc.Top);

            return new TaskbarSnapshot(bounds, ToTaskbarEdge(appBarData.uEdge));
        }

        return GetFallbackSnapshot();
    }

    private static TaskbarSnapshot GetFallbackSnapshot()
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var workArea = SystemParameters.WorkArea;

        if (workArea.Bottom < screenHeight)
        {
            return new TaskbarSnapshot(
                new DesktopRect(0, workArea.Bottom, screenWidth, screenHeight - workArea.Bottom),
                TaskbarEdge.Bottom);
        }

        if (workArea.Top > 0)
        {
            return new TaskbarSnapshot(
                new DesktopRect(0, 0, screenWidth, workArea.Top),
                TaskbarEdge.Top);
        }

        if (workArea.Left > 0)
        {
            return new TaskbarSnapshot(
                new DesktopRect(0, 0, workArea.Left, screenHeight),
                TaskbarEdge.Left);
        }

        if (workArea.Right < screenWidth)
        {
            return new TaskbarSnapshot(
                new DesktopRect(workArea.Right, 0, screenWidth - workArea.Right, screenHeight),
                TaskbarEdge.Right);
        }

        return new TaskbarSnapshot(new DesktopRect(0, screenHeight - 48, screenWidth, 48), TaskbarEdge.Bottom);
    }

    private static TaskbarEdge ToTaskbarEdge(uint edge)
    {
        return edge switch
        {
            0 => TaskbarEdge.Left,
            1 => TaskbarEdge.Top,
            2 => TaskbarEdge.Right,
            _ => TaskbarEdge.Bottom
        };
    }

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern IntPtr SHAppBarMessage(uint dwMessage, ref AppBarData pData);

    [StructLayout(LayoutKind.Sequential)]
    private struct AppBarData
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public Rect rc;
        public IntPtr lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
