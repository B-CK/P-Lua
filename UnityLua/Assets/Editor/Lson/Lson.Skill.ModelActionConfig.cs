using System;
using System.Collections.Generic;

namespace Lson.Skill
{
	public class ModelActionConfig : LsonObject
	{
		/// <summary>
		/// 模型名称
		/// <summary>
		public string ModelName;
		/// <summary>
		/// 模型分组类型
		/// <summary>
		public Lson.Skill.GroupType GroupType;
		/// <summary>
		/// 基础模型名称
		/// <summary>
		public string BaseModelName;
		/// <summary>
		/// 普通动作
		/// <summary>
		public List<Lson.Skill.ModelAction> ModelActionList;
		/// <summary>
		/// 技能动作
		/// <summary>
		public List<Lson.Skill.SkillAction> SkillActionList;
	}
}
