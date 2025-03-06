// Copyright © 2019-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

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
