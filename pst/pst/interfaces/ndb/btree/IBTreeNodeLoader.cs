﻿using pst.core;

namespace pst.interfaces.ndb.btree
{
    interface IBTreeNodeLoader<TNode, TNodeReference>
        where TNode : class
    {
        Maybe<TNode> LoadNode(TNodeReference nodeReference);
    }
}
