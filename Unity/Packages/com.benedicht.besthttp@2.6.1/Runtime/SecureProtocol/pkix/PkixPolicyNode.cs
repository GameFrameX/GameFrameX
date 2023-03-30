#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;
using System.Text;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Collections;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Pkix
{
	/// <summary>
	/// Summary description for PkixPolicyNode.
	/// </summary>
	public class PkixPolicyNode
//		: IPolicyNode
	{
		protected IList				mChildren;
		protected int				mDepth;
		protected ISet				mExpectedPolicies;
		protected PkixPolicyNode	mParent;
		protected ISet				mPolicyQualifiers;
		protected string			mValidPolicy;
		protected bool				mCritical;

		public virtual int Depth
		{
			get { return this.mDepth; }
		}

		public virtual IEnumerable Children
		{
			get { return new EnumerableProxy(mChildren); }
		}

		public virtual bool IsCritical
		{
			get { return this.mCritical; }
			set { this.mCritical = value; }
		}

		public virtual ISet PolicyQualifiers
		{
			get { return new HashSet(this.mPolicyQualifiers); }
		}

		public virtual string ValidPolicy
		{
			get { return this.mValidPolicy; }
		}

		public virtual bool HasChildren
		{
			get { return mChildren.Count != 0; }
		}

		public virtual ISet ExpectedPolicies
		{
			get { return new HashSet(this.mExpectedPolicies); }
			set { this.mExpectedPolicies = new HashSet(value); }
		}

		public virtual PkixPolicyNode Parent
		{
			get { return this.mParent; }
			set { this.mParent = value; }
		}

		/// Constructors
		public PkixPolicyNode(
			IList			children,
			int				depth,
			ISet			expectedPolicies,
			PkixPolicyNode	parent,
			ISet			policyQualifiers,
			string			validPolicy,
			bool			critical)
		{
            if (children == null)
            {
                this.mChildren = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateArrayList();
            }
            else
            {
                this.mChildren = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateArrayList(children);
            }

            this.mDepth = depth;
			this.mExpectedPolicies = expectedPolicies;
			this.mParent = parent;
			this.mPolicyQualifiers = policyQualifiers;
			this.mValidPolicy = validPolicy;
			this.mCritical = critical;
		}

		public virtual void AddChild(
			PkixPolicyNode child)
		{
			child.Parent = this;
			mChildren.Add(child);
		}

		public virtual void RemoveChild(
			PkixPolicyNode child)
		{
			mChildren.Remove(child);
		}

		public override string ToString()
		{
			return ToString("");
		}

		public virtual string ToString(
			string indent)
		{
			StringBuilder buf = new StringBuilder();
			buf.Append(indent);
			buf.Append(mValidPolicy);
			buf.Append(" {");
			buf.Append(BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.NewLine);

			foreach (PkixPolicyNode child in mChildren)
			{
				buf.Append(child.ToString(indent + "    "));
			}

			buf.Append(indent);
			buf.Append("}");
			buf.Append(BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.NewLine);
			return buf.ToString();
		}

		public virtual object Clone()
		{
			return Copy();
		}

		public virtual PkixPolicyNode Copy()
		{
			PkixPolicyNode node = new PkixPolicyNode(
                BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateArrayList(),
				mDepth,
				new HashSet(mExpectedPolicies),
				null,
				new HashSet(mPolicyQualifiers),
				mValidPolicy,
				mCritical);

			foreach (PkixPolicyNode child in mChildren)
			{
				PkixPolicyNode copy = child.Copy();
				copy.Parent = node;
				node.AddChild(copy);
			}

			return node;
		}
	}
}
#pragma warning restore
#endif
