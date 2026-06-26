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
