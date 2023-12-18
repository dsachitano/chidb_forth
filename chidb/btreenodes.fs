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
    btreeNodeStructAddr btree_getPageType
    PGTYPE_TABLE_INTERNAL =
    IF
        btreeNodeStructAddr writeStructRightPageToBlock
    ENDIF 
    update save-buffers
    \ save-buffers
;

\ given a current numCells and a new cellNum idx, make sure that
\ the idx isn't outside of the current range or range + 1
: validateCellNum ( numCells cellNum -- )
    -
    0 <
    IF 
        ." Cellnum is out of range" throw
    ENDIF
;

\ here we need to find the target addr in the cellOffset array
\ then "shift" all of the rest of the array forward if needed,
\ then write the new offset into the spot for cellNum, and then
\ update the free space offset
: updateCellOffsetArray { newCellOffset cellNum btreeNodeAddr -- }


    \ pre: validate that cellNum is within the numCells
    btreeNodeAddr btree_getNumCells cellNum validateCellNum
    
    \ 0. find whether any cellOffset entries need to be moved to make space
    btreeNodeAddr btree_getNumCells                 ( -- numCells )
    cellNum                                         ( numCells -- numCells cellNum )
    -                                               ( numCells cellNum -- cellsToShift )
                                                    \ this is number of cells, starting at idx cellNum, to move up by 2
    btreeNodeAddr btree_getCellOffsetArrayPtr cellNum 2 * +     ( cellsToShift -- cellsToShift addrOfCellNumIdx )
    dup                                             ( cellsToShift addrOfCellNumIdx -- cellsToShift addrOfCellNumIdx addrOfCellNumIdx )
    2 +                                             ( cellsToShift addrOfCellNumIdx addrOfCellNumIdx -- cellsToShift addrOfCellNumIdx addrOfCellNextIdx )
    rot                                             ( cellsToShift addrOfCellNumIdx addrOfCellNextIdx -- addrOfCellNumIdx addrOfCellNextIdx cellsToShift )
    2 *                                             ( addrOfCellNumIdx addrOfCellNextIdx cellsToShift -- addrOfCellNumIdx addrOfCellNextIdx bytesToShift )
    cmove                                           ( addrOfCellNumIdx addrOfCellNextIdx bytesToShift -- )
    update \ Note: I hade a flush here before, and that totally messes up the buffers.



    \ 1. find the location of cellOffsetArrayEntry for cellNum
    btreeNodeAddr btree_getCellOffsetArrayPtr       ( -- cellOffsetArray )
    cellNum 2 *                                     ( cellOffsetArray -- cellOffsetArray cellEntryOffset )
    +                                               ( cellOffsetArray cellEntryOffset -- cellOffsetTargetAddr )
    newCellOffset                                   ( cellOffsetTargetAddr -- cellOffsetTargetAddr newCellOffsetVal )
    swap                                            ( cellOffsetTargetAddr newCellOffsetVal  -- newCellOffsetVal cellOffsetTargetAddr )
    2 writeMultiByteNum                             ( newCellOffsetVal cellOffsetTargetAddr -- )
    update save-buffers

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
: chidb_Btree_insertCell { btreeNodeAddr cellNum cellAddr -- }
    \ 1: get cellsOffset, then subtract by size of cell
    btreeNodeAddr btree_getCellsOffset  ( -- cellsOffset )  
    cellAddr tableCell_getBlockSize     ( cellsOffset -- cellsOffset cellSize )
    -                                   ( cellsOffset cellSize -- newCellOffset )
    dup                                 ( newCellOffset -- newCellOffset newCellOffset)

    \ 1b: add the newCellOffset from above to the pageAddr
    btreeNodeAddr btree_getPageNum block    ( newCellOffset newCellOffset -- newCellOffset newCellOffset pageAddr )
    +                                       ( newCellOffset newCellOffset pageAddr -- newCellOffset pageCellAddrTo )
    cellAddr 1 +                            ( newCellOffset pageCellAddrTo -- newCellOffset pageCellAddrTo cellMemAddrFrom )
    swap                                    ( newCellOffset pageCellAddrTo cellMemAddrFrom -- newCellOffset cellMemAddrFrom pageCellAddrTo )

    \ now we'll copy over 8 bytes from the in-mem struct to the page
    \ since regadless of type, we always have the first 8 bytes identical
    \ between in-memory struct and on-page layout.  After this, we'll need
    \ to do some extra work if it's a leaf node: we need to get the record size,
    \ and the record addr, and then write recordSize bytes from the recordAddr into 
    \ the end of what we just wrote
    8                                       ( newCellOffset cellMemAddrFrom pageCellAddrTo -- newCellOffset cellMemAddrFrom pageCellAddrTo 8 )
    cmove                                   ( newCellOffset cellMemAddrFrom pageCellAddrTo 8 -- newCellOffset )

    dup                                     ( newCellOffset -- newCellOffset newCellOffset )
    btreeNodeAddr btree_getPageType         ( newCellOffset newCellOffset -- newCellOffset newCellOffset pageType )
    PGTYPE_TABLE_LEAF = 
    IF
        ( newCellOffset newCellOffset -- )
        cellAddr tableCell_leaf_getRecordAddr  ( newCellOffset newCellOffset -- newCellOffset newCellOffset recordFromAddr )
        swap                                ( newCellOffset newCellOffset recordFromAddr -- newCellOffset recordFromAddr newCellOffset)
        8 +                                 ( newCellOffset recordFromAddr newCellOffset -- newCellOffset recordFromAddr blockRecordAddr )
        btreeNodeAddr btree_getPageNum block ( newCellOffset recordFromAddr blockRecordAddr -- newCellOffset recordFromAddr blockRecordAddr pageAddr )
        +                                   ( newCellOffset recordFromAddr blockRecordAddr pageAddr -- newCellOffset recordFromAddr blockRecordPageAddr )
        cellAddr tableCell_leaf_getRecordSize ( newCellOffset recordFromAddr blockRecordPageAddr -- newCellOffset recordFromAddr blockRecordPageAddr recordSize )
        cmove                               ( newCellOffset recordFromAddr blockRecordPageAddr recordSize  -- newCellOffset )
    ELSE
        drop                                ( newCellOffset newCellOffset -- newCellOffset )
    ENDIF 

    \ 2: update the cellsOffset
    dup                                     ( newCellOffset -- newCellOffset newCellOffset)
    btreeNodeAddr btree_setCellsOffset      ( newCellOffset newCellOffset -- newCellOffset )

    \ 3: insert the new offset at pos cellNum 
    cellNum btreeNodeAddr updateCellOffsetArray                   ( newCellOffset cellNum btreeNodeAddr --)

    \ 4: incr numCells in the btreeNode
    btreeNodeAddr btree_getNumCells 1 + btreeNodeAddr btree_setNumCells
    btreeNodeAddr chidb_Btree_writeNode
 
;

: loadCellIntoStruct { addrInBlockOfCell pageType bTreeCellAddr -- }
    \ write the page type into the cell struct type (1 byte)
    pageType bTreeCellAddr tableCell_setType

    \ the first 8 bytes of the cell should be the same always, regardless of type
    \ and then if it's a leaf, we need to write 4 more bytes as pointer to the record
    addrInBlockOfCell bTreeCellAddr 1 + 8 cmove 

    pageType PGTYPE_TABLE_LEAF = 
    IF
        addrInBlockOfCell 8 +   ( -- addrInPageOfDBRecord )
        bTreeCellAddr 9 +       ( -- addrInPageOfDBRecord recordPtrInStruct )
        4 writeMultiByteNum
    ENDIF
;

\ /* Read the contents of a cell
\  *
\  * Reads the contents of a cell from a BTreeNode and stores them in a BTreeCell.
\  * This involves the following:
\  *  1. Find out the offset of the requested cell.
\  *  2. Read the cell from the in-memory page, and parse its
\  *     contents (refer to The chidb File Format document for
\  *     the format of cells).
\  *
\  * Parameters
\  * - btn: BTreeNode where cell is contained
\  * - ncell: Cell number
\  * - cell: BTreeCell where contents must be stored.
\  *
\  * Return
\  * - CHIDB_OK: Operation successful
\  * - CHIDB_ECELLNO: The provided cell number is invalid
\  */
: chidb_Btree_getCell { btreeNodeAddr cellNum bTreeCellAddr -- }
    \ 1. get the address of the cell.  First, find the cellOffsets array, then lookup cellNum in it.
    \    then the value should be an offset from the start of the page, which we use to compute the
    \    address of the cell data.
    btreeNodeAddr btree_getCellOffsetArrayPtr   ( -- cellOffsetArrayBase )
    cellNum 2 *                                 ( -- cellOffsetArrayBase cellIdx ) \ 2 bytes per entry
    +                                           ( -- cellOffsetEntryAddr )
    2 multiByteNum                              ( -- valueOfCellOffset )

    btreeNodeAddr btree_getPageNum              ( -- valueOfCellOffset pageNum )
    dup                                         ( -- valueOfCellOffset pageNum pageNum )
    1 =                                         
    IF
        \ for page 1, the addr is blockAddr - 100 (to get us to the page addr)
        \ then add the offset
        btree_blockAddr                             ( valueOfCellOffset pagenum -- valueOfCellOffset blockAddr )
        100 -
        +
    ELSE
        \ for page > 1, the addr is just blockAddr plus offset
        btree_blockAddr + 
    ENDIF

    ( -- addrInBlockOfCell )

    \ 2. load the contents of the cell from the page into the bTreeCellAddr
    btreeNodeAddr btree_getPageType     ( addInBlockOfCell -- addInBlockOfCell pageType )
    bTreeCellAddr loadCellIntoStruct
;