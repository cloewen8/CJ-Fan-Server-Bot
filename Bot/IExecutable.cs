﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
	public interface IExecutable<T>
	{
		Task Execute(T input);
	}
}
