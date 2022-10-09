using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace STLio
{
    public class STLioInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "STLio";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("8b35cda9-0744-4fa5-880d-38308ed7a4b9");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
