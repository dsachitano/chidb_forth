s" globalsAndConsts.fs" required
s" utils.fs" required

\ /* The BTreeNode struct is an in-memory representation of a B-Tree node. Thus,
\  * most of the values in this struct are simply a copy, for ease of access,
\  * of what can be found in the raw disk page. When modifying type, free_offset,
\  * n_cells, cells_offset, or right_page, do so in the corresponding field
\  * of the BTreeNode variable (the changes will be effective once the BTreeNode
\  * is written to disk, using chidb_Btree_writeNode). Modifications of the
\  * cell offset array or of the cells should be done directly on the in-memory
\  * page returned by the Pager.
\  *
\  * See The chidb File Format document for more details on the meaning of each
\  * field.
\  */
\ struct BTreeNode
\ {
\     MemPage *page;             /* In-memory page returned by the Pager */
\     uint8_t type;              /* Type of page  */
\     uint16_t free_offset;      /* Byte offset of free space in page */
\     ncell_t n_cells;           /* Number of cells */
\     uint16_t cells_offset;     /* Byte offset of start of cells in page */
\     npage_t right_page;        /* Right page (internal nodes only) */
\     uint8_t *celloffset_array; /* Pointer to start of cell offset array in the in-memory page */
\ };

: bTreeNodeSize ( -- sizeInBytes )
    4           \ 4-bytes for pageNum                               offset: 0
    1 +         \ 1 byte for pageType                               offset: 4
    2 +         \ 2-bytes for freeOffset                            offset: 5
    2 +         \ 2-bytes for numCells                              offset: 7
    2 +         \ 2-bytes for cellsOffset                           offset: 9
    4 +         \ 4-bytes for rightPage (internal nodes only)       offset: 11
    4 +         \ addr of start of cell offset array in the page    offset: 15
;

: allocateBtreeNode ( -- addr) 
    bTreeNodeSize allocate        ( -- addr wior)

    IF
        ." failed to allocate memory in allocateBtreeNode"
    ENDIF      

    dup             ( addr -- addr addr)
    19 32 fill      ( addr addr 19 32 -- addr)  \ initialize each byte to 0x20
;

: dumpNode ( addr -- addr )
    dup         ( addr -- addr addr )
    bTreeNodeSize dump ( addr addr size -- addr )
;

: btree_getPageNum ( btreeAddr -- pageNum )
    4 multiByteNum
;

: btree_setPageNum ( val btreeAddr -- )
    4 writeMultiByteNum
;

: btree_getPageType ( btreeAddr -- pageType )
    4 +             \ offset into the struct
    c@              \ get oneByte
;

: btree_setPageType ( val btreeAddr -- )
    4 +             \ offset into the struct
    c! 
;

: btree_getFreeOffset ( btreeAddr -- pageNum )
    5 +             \ offset into the struct
    2 multiByteNum
;

: btree_setFreeOffset ( val btreeAddr -- )
    5 +             \ offset into the struct
    2 writeMultiByteNum
;

: btree_getNumCells ( btreeAddr -- pageNum )
    7 +             \ offset into the struct
    2 multiByteNum
;

: btree_setNumCells ( val btreeAddr -- )
    7 +             \ offset into the struct
    2 writeMultiByteNum
;

: btree_getCellsOffset ( btreeAddr -- pageNum )
    9 +             \ offset into the struct
    2 multiByteNum
;

: btree_setCellsOffset ( val btreeAddr -- )
    9 +             \ offset into the struct
    2 writeMultiByteNum
;

: btree_getRightPage ( btreeAddr -- pageNum )
    11 +             \ offset into the struct
    4 multiByteNum
;

: btree_setRightPage ( val btreeAddr -- )
    11 +             \ offset into the struct
    4 writeMultiByteNum
;

: btree_getCellOffsetArrayPtr ( btreeAddr -- pageNum )
    15 +             \ offset into the struct
    4 multiByteNum
;

: btree_setCellOffsetArrayPtr ( val btreeAddr -- )
    15 +             \ offset into the struct
    4 writeMultiByteNum
;