/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/
namespace PolymorphicStructsTests
{
	
	public partial struct CorrectImplementationA
	{
		
		public CorrectImplementationA(CorrectImplementationInterface s)
		{
			A = s.Int32_0;
		}
		
		public void ToCorrectImplementationInterface(ref CorrectImplementationInterface s)
		{
			s.CurrentTypeId = CorrectImplementationInterface.TypeId.CorrectImplementationA;
			s.Int32_0 = A;
		}
		
		public CorrectImplementationInterface ToCorrectImplementationInterface()
		{
			var s = new CorrectImplementationInterface();
			ToCorrectImplementationInterface(ref s);
			return s;
		}
	}
}
