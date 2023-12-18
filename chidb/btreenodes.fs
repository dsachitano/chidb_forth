s" globalsAndConsts.fs" required

\ 
\ Step 2: Loading a B-Tree node from the file
\ ---------------------------------------------------------------------
\ int chidb_Btree_getNodeByPage(BTree *bt, npage_t npage, BTreeNode **node);
\ int chidb_Btree_freeMemNode(BTree *bt, BTreeNode *btn);
\
\ Step 3: Creating and writing a B-Tree node to disk
\ ---------------------------------------------------------------------
\ int chidb_Btree_newNode(BTree *bt, npage_t *npage, uint8_t type);
\ int chidb_Btree_initEmptyNode(BTree *bt, npage_t npage, uint8_t type);
\ int chidb_Btree_writeNode(BTree *bt, BTreeNode *node);

: btree_blockAddr ( pagenum -- blockAddr )
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

\ two byte offset updated each time celloffset array is modified
: writeFreeOffset ( offsetVal pageAddr -- )
    1 +                 ( offsetVal addr -- )
    2                   ( offsetVal addr 2 -- )
    writeMultiByteNum   ( offsetVal addr 2 -- )
;

\ in a new pageHeadeer, with no cells yet, the freeOffset starts at end of header
: initializeFreeOffset ( pageType pageAddr -- )
    swap                ( pageAddr pageType -- )
    PGTYPE_TABLE_INTERNAL = 
    IF
        12 swap writeFreeOffset     \ use 12 (first byte after header) for internal
    ELSE 
        \ this else case assumes PGTYPE_TABLE_LEAF
        8 swap writeFreeOffset      \ use 8 (first byte after header, no right page ptr) for leaf
    ENDIF
;

: writeNumCells ( numCells pageAddr -- )
    3 +         ( numCells numCellsOffset -- )
    2           ( numCells numCellsOffset 2 -- )
    writeMultiByteNum
;

: writeCellsOffset ( cellsOffset pageAddr -- )
    5 +         ( cellsOffset cellsOffsetHeaderPos -- )
    2           ( cellsOffset cellsOffsetHeaderPos 2 -- )
    writeMultiByteNum 
;

: writeRightPage    ( pageNum pageAddr -- )
    8 +             ( pageNum pageNumHeaderPos -- )
    4               ( pageNum pageNumHeaderPos 4 -- )
    writeMultiByteNum 
;

: writePageHeader { pageType pageAddr -- pageAddr }
    pageType pageAddr   writePageType
    pageType pageAddr   initializeFreeOffset
    0 pageAddr          writeNumCells
    pageSize @ pageAddr writeCellsOffset
    0 pageAddr 7 +      c!  \ write a zero at the offset 7 in the pageHeader, per spec
    pageAddr
;

: btree_block_getPageType ( btreeBlockHeaderAddr -- pageType )
    c@
;

\ note: this is a byteOffset within the page, starting from header
: btree_block_getFreeOffset ( btreeBlockHeaderAddr -- freeOffset )
    1 +                 \ freeOffset is at header offset 1
    2 multiByteNum      \ it's a 2-byte number
;

: btree_block_getNumCells ( btreeBlockHeaderAddr -- numCells )
    3 +
    2 multiByteNum
;

: btree_block_getCellsOffset
    5 +
    2 multiByteNum
;

: btree_block_getRightPage
    8 +
    4 multiByteNum
;

: btree_block_getCellOffsetArray ( blockAddr -- cellOffsetAddr )
    dup                       ( -- blockAddr blockAddr) 
    btree_block_getPageType   ( -- blockAddr pageType)
    PGTYPE_TABLE_INTERNAL =
    IF
        12 +
    ELSE
        8 +
    ENDIF
;

: loadBlockIntoStruct { structAddr blockAddr pageNum -- structAddr)

    \ pageNum
    pageNum structAddr btree_setPageNum

    \ pageType
    blockAddr btree_block_getPageType       ( -- pageType)
    structAddr btree_setPageType             ( pageType -- )

    \ freeOffset
    blockAddr btree_block_getFreeOffset
    structAddr btree_setFreeOffset

    \ numCells
    blockAddr btree_block_getNumCells
    structAddr btree_setNumCells

    \ cellsOffset
    blockAddr btree_block_getCellsOffset
    structAddr btree_setCellsOffset

    \ IF we have an internal node, copy in the right page
    structAddr btree_getPageType 
    PGTYPE_TABLE_INTERNAL =
    IF
        blockAddr btree_block_getRightPage
        structAddr btree_setRightPage
    ENDIF

    \ cellOffsetArray ptr to the block addr immediately after header
    blockAddr btree_block_getCellOffsetArray
    structAddr btree_setCellOffsetArrayPtr

    structAddr 
;

\ here we'll take the address of a block buffer for the node
\ and then allocate a new btree node struct, and load it with
\ the data from the block on disk, and return the node struct addr
: btreeStructFromBlockAddr { blockAddr pageNum -- structAddr}
    allocateBtreeNode       ( -- structAddr )
    blockAddr               ( structAddr -- structAddr blockAddr )
    pageNum loadBlockIntoStruct     ( structAddr blockAddr pageNum -- )
;

\ int chidb_Btree_getNodeByPage(BTree *bt, npage_t npage, BTreeNode **node);
\ here, we'll just assume one btree (blockfile) at a time, and so then page
\ number is just a block number
: chidb_Btree_getNodeByPage { pagenum -- btreeNodeAddr)
    pagenum btree_blockAddr             ( -- blockAddr)
    pagenum btreeStructFromBlockAddr    ( blockAddr pageNum -- btreeNodeAddr )
;

: chidb_Btree_initEmptyNode ( pageType pageNum -- )
    btree_blockAddr     ( pageType pageNum -- pageType blockAddr )
    writePageHeader     ( pageType blockAddr -- pageAddr)
    drop
;

: chidb_Btree_newNode ( type -- )
    \ first, incr global numPages variable
    1 numPages +!               ( type -- type )
    numPages @                  ( type -- type pageNum )
    chidb_Btree_initEmptyNode   ( type pageNum -- )
;

: getBlockAddrForStruct ( btreeNodeStructAddr -- blockAddr )
    btree_getPageNum    ( btreeNodeStructAddr -- pageNum )
    btree_blockAddr     ( pageNum -- blockAddr )
;

: writeStructPageTypeToBlock ( btreeNodeStructAddr -- )
    dup                   ( btreeNodeStructAddr -- btreeNodeStructAddr btreeNodeStructAddr)
    btree_getPageType     ( btreeNodeStructAddr btreeNodeStructAddr -- btreeNodeStructAddr pageType )
    swap                  ( btreeNodeStructAddr pageType -- pageType btreeNodeStructAddr )
    getBlockAddrForStruct ( pageType btreeNodeStructAddr -- pageType pageAddr )
    writePageType         ( pageType pageAddr -- )
;

: writeStructFreeOffsetToBlock ( btreeNodeStructAddr -- )
    dup                 ( btreeNodeStructAddr -- btreeNodeStructAddr btreeNodeStructAddr)
    btree_getFreeOffset ( btreeNodeStructAddr btreeNodeStructAddr -- btreeNodeStructAddr offsetVal )
    swap                ( btreeNodeStructAddr offsetVal -- offsetVal btreeNodeStructAddr )
    getBlockAddrForStruct ( offsetVal btreeNodeStructAddr -- offsetVal pageAddr )
    writeFreeOffset     ( offsetVal pageAddr -- )
;

: writeStructNumCellsToBlock ( btreeNodeStructAddr -- )
    dup                 ( btreeNodeStructAddr -- btreeNodeStructAddr btreeNodeStructAddr )
    btree_getNumCells   ( btreeNodeStructAddr btreeNodeStructAddr -- btreeNodeStructAddr numCells )
    swap                ( btreeNodeStructAddr numCells -- numCells btreeNodeStructAddr )
    getBlockAddrForStruct ( numCells btreeNodeStructAddr -- numCells pageAddr )
    writeNumCells       ( numCells pageAddr -- )
;

: writeStructCellsOffsetToBlock ( btreeNodeStructAddr -- )
    dup                     ( btreeNodeStructAddr -- btreeNodeStructAddr btreeNodeStructAddr )
    btree_getCellsOffset    ( btreeNodeStructAddr btreeNodeStructAddr -- btreeNodeStructAddr cellsOffset )
    swap                    ( btreeNodeStructAddr cellsOffset -- cellsOffset btreeNodeStructAddr )
    getBlockAddrForStruct   ( cellsOffset btreeNodeStructAddr -- cellsOffset pageAddr )
    writeCellsOffset        ( cellsOffset pageAddr -- )
;

: writeStructRightPageToBlock ( btreeNodeStructAddr -- )
    dup                     ( btreeNodeStructAddr -- btreeNodeStructAddr btreeNodeStructAddr )
    btree_getRightPage      ( btreeNodeStructAddr btreeNodeStructAddr -- btreeNodeStructAddr pageNum )
    swap                    ( btreeNodeStructAddr pageNum -- pageNum btreeNodeStructAddr )
    getBlockAddrForStruct   ( cellsOffset btreeNodeStructAddr -- pageNum pageAddr )
    writeRightPage          ( pageNum pageAddr -- )
;

\ take a btree node struct and write to block
: chidb_Btree_writeNode { btreeNodeStructAddr -- )
    btreeNodeStructAddr writeStructPageTypeToBlock
    btreeNodeStructAddr writeStructFreeOffsetToBlock
    btreeNodeStructAddr writeStructNumCellsToBlock
    btreeNodeStructAddr writeStructCellsOffsetToBlock
    btreeNodeStructAddr writeStructRightPageToBlock
;

\  * Inserts a new cell into a B-Tree node at a specified position ncell.
\  * This involves the following:
\  *  1. Add the cell at the top of the cell area. This involves "translating"
\  *     the BTreeCell into the chidb format (refer to The chidb File Format
\  *     document for the format of cells).
\  *  2. Modify cells_offset in BTreeNode to reflect the growth in the cell area.
\  *  3. Modify the cell offset array so that all values in positions >= ncell
\  *     are shifted one position forward in the array. Then, set the value of
\  *     position ncell to be the offset of the newly added cell.
: chidb_Btree_insertCell ( btreeNodeAddr cellNum cellAddr -- )
    
;