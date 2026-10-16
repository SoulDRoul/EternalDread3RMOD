using System.Collections.Generic;

namespace EternalDread3RMOD
{
    public static class MinimapIconRegistry
    {
        public static readonly List<MinimapIcon> EnemyIcons
            = new List<MinimapIcon>();

        public static void Register(MinimapIcon icon)
        {
            if (icon == null) return;
            if (EnemyIcons.Contains(icon)) return;
            EnemyIcons.Add(icon);
        }

        public static void Unregister(MinimapIcon icon)
        {
            if (icon == null) return;
            EnemyIcons.Remove(icon);
        }

        public static void RefreshAllEnemies()
        {
            for (int i = EnemyIcons.Count - 1; i >= 0; i--)
            {
                var icon = EnemyIcons[i];
                if (icon == null)
                {
                    EnemyIcons.RemoveAt(i);
                    continue;
                }
                icon.UpdateEnemyColor();
            }
        }
    }
}