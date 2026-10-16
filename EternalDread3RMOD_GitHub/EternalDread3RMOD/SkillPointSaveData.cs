using System.Collections.Generic;
using System.Xml.Serialization;

namespace EternalDread3RMOD
{
    [XmlRoot("SkillPointSave")]
    public class SkillPointSaveData
    {
        [XmlArray("Records")]
        [XmlArrayItem("Record")]
        public List<SkillPointRecord> Records = new List<SkillPointRecord>();

        public int TodayRefreshCount = 0;
        public int TodayDayNumber = -1;
    }

    public class SkillPointRecord
    {
        [XmlAttribute]
        public int BoolId;

        [XmlAttribute]
        public float LastPickupGameHours = -1f;

        /// <summary>已刷出但未拾取。true 时场景加载中保留物体，不再走概率判定。</summary>
        [XmlAttribute]
        public bool IsSpawned = false;
    }
}