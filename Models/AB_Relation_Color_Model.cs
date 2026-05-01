using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>관계 유형 → 적대도(색상) 매핑.</summary>
    [Table("circuit_relation_colors")]
    public class AB_Relation_Color_Model
    {
        private string m_id_ = Guid.NewGuid().ToString();
        [Key]
        [Column("id")]
        public string Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>관계 유형 이름 (예: "동료", "friend", "적")</summary>
        private string m_relationType_ = "";
        [Column("relation_type")]
        public string RelationType_
        {
            get { return m_relationType_; }
            set { m_relationType_ = value; }
        }

        /// <summary>적대도 0.0(우호/파랑) ~ 1.0(적대/빨강)</summary>
        private double m_hostility_ = 0.5;
        [Column("hostility")]
        public double Hostility_
        {
            get { return m_hostility_; }
            set { m_hostility_ = value; }
        }
    }
}
