using System.Collections.Generic;
using UnityEngine;

public static class UIStackManager
{
    private static readonly List<ICloseableUI> _activeUIs = new List<ICloseableUI>();

    public static bool HasActiveUI
    {
        get
        {
            CleanDeadReferences();
            return _activeUIs.Count > 0;
        }
    }

    public static int ActiveUICount
    {
        get
        {
            CleanDeadReferences();
            return _activeUIs.Count;
        }
    }

    public static void RegisterOpen(ICloseableUI ui)
    {
        if (ui == null) return;

        CleanDeadReferences();

        _activeUIs.Remove(ui);
        _activeUIs.Add(ui);

        EventBus<ChangeActionMap>.Raise(new ChangeActionMap { MapType = ActionMapType.UI });
    }

    public static void UnregisterClosed(ICloseableUI ui)
    {
        if (ui == null) return;

        _activeUIs.Remove(ui);
        CleanDeadReferences();

        if (_activeUIs.Count == 0)
        {
            EventBus<ChangeActionMap>.Raise(new ChangeActionMap { MapType = ActionMapType.Player });
        }
    }

    public static bool TryCloseTopUI()
    {
        CleanDeadReferences();

        while (_activeUIs.Count > 0)
        {
            int lastIndex = _activeUIs.Count - 1;
            var top = _activeUIs[lastIndex];
            _activeUIs.RemoveAt(lastIndex);

            if (top != null && top.IsOpen)
            {
                top.CloseUI();
                CleanDeadReferences();

                if (_activeUIs.Count == 0)
                {
                    EventBus<ChangeActionMap>.Raise(new ChangeActionMap { MapType = ActionMapType.Player });
                }
                return true;
            }
        }

        return false;
    }

    public static void CheckAndSyncActionMap()
    {
        CleanDeadReferences();
        if (_activeUIs.Count == 0)
        {
            EventBus<ChangeActionMap>.Raise(new ChangeActionMap { MapType = ActionMapType.Player });
        }
        else
        {
            EventBus<ChangeActionMap>.Raise(new ChangeActionMap { MapType = ActionMapType.UI });
        }
    }

    private static void CleanDeadReferences()
    {
        for (int i = _activeUIs.Count - 1; i >= 0; i--)
        {
            var ui = _activeUIs[i];
            if (ui == null || (ui is Object unityObj && unityObj == null) || !ui.IsOpen)
            {
                _activeUIs.RemoveAt(i);
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        _activeUIs.Clear();
    }
}
