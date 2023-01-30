/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/

namespace PolymorphicStructsTests
{
	public partial struct VoidMethodB
	{
		public VoidMethodB(VoidMethodInterface s)
		{
			B = s.Int32_0;
		}
		public void ToVoidMethodInterface(ref VoidMethodInterface s)
		{
			s.CurrentTypeId = VoidMethodInterface.TypeId.VoidMethodB;
			s.Int32_0 = B;
		}
		public VoidMethodInterface ToVoidMethodInterface()
		{
			var s = new VoidMethodInterface();
			ToVoidMethodInterface(ref s);
			return s;
		}
	}
}
