\ Step 3: Creating and writing a B-Tree node to disk
\ ---------------------------------------------------------------------
\ int chidb_Btree_newNode(BTree *bt, npage_t *npage, uint8_t type);
\ int chidb_Btree_initEmptyNode(BTree *bt, npage_t npage, uint8_t type);
\ int chidb_Btree_writeNode(BTree *bt, BTreeNode *node);

\ int chidb_Btree_getNodeByPage(BTree *bt, npage_t npage, BTreeNode **node);
\ here, we'll just assume one btree (blockfile) at a time, and so then page
\ number is just a block number
: chidb_Btree_getNodeByPage ( pagenum -- blockAddr)
    dup     ( pagenum pagenum -- ) 
    1 =     ( pagenum -- )
    IF
        block 100 +
    ELSE
        block
    ENDIF
;

: writePageType ( pageType pageAddr -- )
    \ ." writing page type: " .s 
    c!      ( -- )
;

: writePageHeader { pageType pageAddr -- }
    pageType pageAddr writePageType
;

: chidb_Btree_newNode ( type pagenum -- )
    chidb_Btree_getNodeByPage   ( type blockAddr -- )
    writePageHeader
;