using System;
using System.Collections.Generic;
using Csv;

namespace Csv.Model
{
	public class Model : CfgObject
	{
		/// <summary>
		/// 模型名
		/// <summary>
		public readonly string Name;
		/// <summary>
		/// 模型级别
		/// <summary>
		public readonly int Level;

		public Model(DataStream data)
		{
			this.Name = data.GetString();
			this.Level = data.GetInt();
		}
	}
}
