﻿using pst.core;
using pst.interfaces.messaging;
using pst.interfaces.ndb;
using pst.utilities;

namespace pst
{
    public class MessageStore
    {
        private readonly IPropertyContextBasedReadOnlyComponent propertyContextBasedReadOnlyComponent;

        internal MessageStore(IPropertyContextBasedReadOnlyComponent propertyContextBasedReadOnlyComponent)
        {
            this.propertyContextBasedReadOnlyComponent = propertyContextBasedReadOnlyComponent;
        }

        public Maybe<PropertyValue> GetProperty(NumericalPropertyTag propertyTag)
        {
            return
                propertyContextBasedReadOnlyComponent.GetProperty(
                    new NumericalTaggedPropertyPath(NodePath.OfValue(Constants.NID_MESSAGE_STORE), propertyTag));
        }

        public Maybe<PropertyValue> GetProperty(StringPropertyTag propertyTag)
        {
            return
                propertyContextBasedReadOnlyComponent.GetProperty(
                    new StringTaggedPropertyPath(NodePath.OfValue(Constants.NID_MESSAGE_STORE), propertyTag));
        }

        public Maybe<PropertyValue> GetProperty(PropertyTag propertyTag)
        {
            return
                propertyContextBasedReadOnlyComponent.GetProperty(
                    new TaggedPropertyPath(NodePath.OfValue(Constants.NID_MESSAGE_STORE), propertyTag));
        }
    }
}
