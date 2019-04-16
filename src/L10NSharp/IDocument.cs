// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace L10NSharp
{
	public interface IDocument
	{
		bool IsDirty { get; }
		void Save(string filename);
		bool AddTransUnit(TransUnit tu);
		void RemoveTransUnit(TransUnit tu);
	}
}
