// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using L10NSharp.XLiffUtils;

namespace L10NSharp
{
	public interface IDocument
	{
		bool IsDirty { get; }
		void Save(string filename);
		bool AddTransUnit(ITransUnit tu);
		void RemoveTransUnit(ITransUnit tu);
	}
}
