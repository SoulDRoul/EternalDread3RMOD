using System.Collections.Generic;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 生态存档的顶层结构。
    /// </summary>
    public class EcologySaveData
    {
        public float TotalGameHours;            // 存档时的累计度过小时
        public string CharName;                 // 参考信息（哪个角色）
        public List<RegionState> Regions;       // 所有区域

        public EcologySaveData()
        {
            TotalGameHours = 0f;
            CharName = string.Empty;
            Regions = new List<RegionState>();
        }
    }
}