﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Brass9
{
	public interface ILog
	{
		Task WriteLineAsync(string line);
		Task LogExceptionAsync(Exception ex);
		Task LogShortExceptionAsync(Exception ex);
	}
}
