using System;
using System.Runtime.Serialization;

namespace PrivilegeClass
{
    [Serializable]
    public sealed class PrivilegeNotHeldException : UnauthorizedAccessException, ISerializable
    {
        public PrivilegeNotHeldException()
            : base("A privilege necessary for this operation to succeed is not held by the caller.")
        {
        }

        public PrivilegeNotHeldException(string privilege)
            : base(string.Format("The {0} privilege is not held by the caller.", privilege))
        {
            this.PrivilegeName = privilege;
        }

        public PrivilegeNotHeldException(string privilege, Exception inner)
            : base(string.Format("The {0} privilege is not held by the caller.", privilege), inner)
        {
            this.PrivilegeName = privilege;
        }

        private PrivilegeNotHeldException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.PrivilegeName = info.GetString("PrivilegeName");
        }

        public string PrivilegeName { get; } = null;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            base.GetObjectData(info, context);

            info.AddValue("PrivilegeName", this.PrivilegeName, typeof(string));
        }
    }
}