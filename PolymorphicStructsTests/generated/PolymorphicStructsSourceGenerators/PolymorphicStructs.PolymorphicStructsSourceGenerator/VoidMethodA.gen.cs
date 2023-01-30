/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/

namespace PolymorphicStructsTests
{
	public partial struct VoidMethodA
	{
		public VoidMethodA(VoidMethodInterface s)
		{
			A = s.Int32_0;
		}
		public void ToVoidMethodInterface(ref VoidMethodInterface s)
		{
			s.CurrentTypeId = VoidMethodInterface.TypeId.VoidMethodA;
			s.Int32_0 = A;
		}
		public VoidMethodInterface ToVoidMethodInterface()
		{
			var s = new VoidMethodInterface();
			ToVoidMethodInterface(ref s);
			return s;
		}
	}
}
