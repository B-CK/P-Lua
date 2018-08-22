using System;
using System.Collections.Generic;
using Csv;

namespace Csv.Skill
{
	public class ModelActionConfig : CfgObject
	{
		/// <summary>
		/// 模型名称
		/// <summary>
		public readonly string ModelName;
		/// <summary>
		/// 模型分组类型
		/// <summary>
		public readonly int GroupType;
		/// <summary>
		/// 基础模型名称
		/// <summary>
		public readonly string BaseModelName;
		/// <summary>
		/// 普通动作
		/// <summary>
		public readonly List<Csv.Skill.ModelAction> ModelActionList = new List<Csv.Skill.ModelAction>();
		/// <summary>
		/// 技能动作
		/// <summary>
		public readonly List<Csv.Skill.SkillAction> SkillActionList = new List<Csv.Skill.SkillAction>();

		public ModelActionConfig(DataStream data)
		{
			this.ModelName = data.GetString();
			this.GroupType = data.GetInt();
			this.BaseModelName = data.GetString();
			for (int n = data.GetInt(); n-- > 0; )
			{
				this.ModelActionList.Add((Skill.ModelAction)data.GetObject(data.GetString()));
			}
			for (int n = data.GetInt(); n-- > 0; )
			{
				this.SkillActionList.Add(new Skill.SkillAction(data));
			}
		}
	}
}
