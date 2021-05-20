using System;
using System.Runtime.Serialization;

namespace PrivilegeClass
{
    [Serializable]
    public sealed class PrivilegeNotHeldException : UnauthorizedAccessException, ISerializable
    {
        private readonly string privilegeName = null;

        public PrivilegeNotHeldException()
            : base("A privilege necessary for this operation to succeed is not held by the caller.")
        {
        }

        public PrivilegeNotHeldException(string privilege)
            : base(string.Format("The {0} privilege is not held by the caller.", privilege))
        {
            this.privilegeName = privilege;
        }

        public PrivilegeNotHeldException(string privilege, Exception inner)
            : base(string.Format("The {0} privilege is not held by the caller.", privilege), inner)
        {
            this.privilegeName = privilege;
        }

        internal PrivilegeNotHeldException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.privilegeName = info.GetString("PrivilegeName");
        }

        public string PrivilegeName
        {
            get { return this.privilegeName; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            base.GetObjectData(info, context);

            info.AddValue("PrivilegeName", this.privilegeName, typeof(string));
        }
    }
}