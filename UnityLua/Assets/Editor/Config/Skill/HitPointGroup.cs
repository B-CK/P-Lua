using System;
using System.Collections.Generic;
using Csv;

namespace Csv.Skill
{
	public class HitPointGroup : CfgObject
	{
		/// <summary>
		/// 打击点组ID
		/// <summary>
		public readonly int Id;
		/// <summary>
		/// 打击点组名称
		/// <summary>
		public readonly string Name;
		/// <summary>
		/// 打击点列表
		/// <summary>
		public readonly List<Csv.Skill.Attack> Attacks = new List<Csv.Skill.Attack>();

		public HitPointGroup(DataStream data)
		{
			this.Id = data.GetInt();
			this.Name = data.GetString();
			for (int n = data.GetInt(); n-- > 0; )
			{
				this.Attacks.Add(new Skill.Attack(data));
			}
		}
	}
}
