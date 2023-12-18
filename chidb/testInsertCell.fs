s" chidb.fs" included

clearstack

assert-level 3

: test_insertCell { btreenodestructaddr cellAddr -- }
    \ cell with childPageNum 2, and key 1, and a record of size 16
    \ 2 cellAddr tableCell_internal_setChildPageNum
    1 cellAddr tableCell_setKey
    16 cellAddr tableCell_leaf_setRecordSize
     
    16 allocate drop    ( -- recordAddr )
    dup                 ( recordAddr -- recordAddr recordAddr )
    16 77 fill          ( recordAddr recordAddr -- recordAddr ) \ initialize the record to 77 in each byte (0x4D)
    cellAddr tableCell_leaf_setRecordAddr

    btreenodestructaddr 0 cellAddr chidb_Btree_insertCell  ( btreeNodeAddr cellNum cellAddr -- )

    btreenodestructaddr chidb_Btree_writeNode 

    \ validate numCells
    btreenodestructaddr btree_getNumCells
    assert( 1 = )

    btreenodestructaddr btree_getCellsOffset 
    assert( 1024 24 - = )

    btreenodestructaddr btree_getCellOffsetArrayPtr
    2 multiByteNum
    assert( 1024 24 - = )  
;

: test_insertCell2 { btreenodestructaddr cellAddr -- }
    \ cell with childPageNum 3, and key 2
    \ 3 cellAddr tableCell_internal_setChildPageNum
    2 cellAddr tableCell_setKey
    16 cellAddr tableCell_leaf_setRecordSize
    16 allocate drop dup 16 61 fill  \ initialize the record to 61 in each byte (0x3D)
    cellAddr tableCell_leaf_setRecordAddr

    btreenodestructaddr 1 cellAddr chidb_Btree_insertCell  ( btreeNodeAddr cellNum cellAddr -- )

    btreenodestructaddr chidb_Btree_writeNode

    \ validate numCells
    btreenodestructaddr btree_getNumCells
    assert( 2 = )

    btreenodestructaddr btree_getCellsOffset
    assert( 1024 48 - = )
;

: validateInsertedCell { cellAddr -- }
    cellAddr tableCell_getType 
    assert( PGTYPE_TABLE_LEAF = )

    cellAddr tableCell_getKey
    assert( 2 = )

    cellAddr tableCell_leaf_getRecordSize
    assert( 16 = )

    cellAddr tableCell_leaf_getRecordAddr
    4 multiByteNum
    assert( 0x3D3D3D3D = )
;

: test_getCell { btreenodestructaddr -- }

    btreenodestructaddr     ( -- btreenodestructaddr)
    allocateTableCellLeaf   ( btreenodestructaddr -- btreenodestructaddr cellAddr )
    dup                     ( btreenodestructaddr cellAddr -- btreenodestructaddr cellAddr cellAddr )
    rot                     ( btreenodestructaddr cellAddr cellAddr -- cellAddr cellAddr btreenodestructaddr )
    swap                    ( cellAddr cellAddr btreenodestructaddr -- cellAddr btreenodestructaddr cellAddr )
    1                       ( cellAddr btreenodestructaddr cellAddr -- cellAddr btreenodestructaddr cellAddr 1)
    swap                    ( cellAddr btreenodestructaddr cellAddr 1 -- cellAddr btreenodestructaddr 1 cellAddr)
    chidb_Btree_getCell     ( cellAddr btreenodestructaddr 1 cellAddr -- cellAddr )
    dup                     ( cellAddr -- cellAddr cellAddr )

        \ 13 dump 

    validateInsertedCell

    free drop 
;

: test_insertCell3 { btreenodestructaddr cellAddr -- }

    3 cellAddr tableCell_setKey
    16 cellAddr tableCell_leaf_setRecordSize
    16 allocate drop dup 16 45 fill  \ initialize the record to 61 in each byte (0x2D)
    cellAddr tableCell_leaf_setRecordAddr

    btreenodestructaddr 1 cellAddr chidb_Btree_insertCell  ( btreeNodeAddr cellNum cellAddr -- )

    btreenodestructaddr chidb_Btree_writeNode

    \ validate numCells
    btreenodestructaddr btree_getNumCells
    assert( 3 = )

    btreenodestructaddr btree_getCellsOffset
    assert( 1024 72 - = )

    \ debug
    \ 1 block 100 + 1024 100 - dump

    \ lookup the cellOffset array, and since we inserted at indexes
    \ 0, 1, 1, then idx 0 should be the end-most, idx 1 should be
    \ the *newest* (3 in from the end), and idx 2 should be the 2nd
    \ one we inserted 48 in from the end
    btreenodestructaddr btree_getCellOffsetArrayPtr
    dup 2 multiByteNum assert( 1024 24 - = )
    dup 2 + 2 multiByteNum assert( 1024 24 3 * - = )
    dup 4 + 2 multiByteNum assert( 1024 24 2 * - = )
    drop 
;

: runTest 
    \ initialize a DB
    s" insertCellTest.db" chidb_Btree_open  ( -- )
    1 chidb_Btree_getNodeByPage dup         ( -- btreenodestructaddr btreenodestructaddr)
    allocateTableCellLeaf               ( btreenodestructaddr btreenodestructaddr -- btreenodestructaddr btreenodestructaddr cellAddr )
    test_insertCell                         ( btreenodestructaddr btreenodestructaddr cellAddr -- btreenodestructaddr )


    dup                                     ( btreenodestructaddr -- btreenodestructaddr btreenodestructaddr )
    allocateTableCellLeaf               ( btreenodestructaddr btreenodestructaddr -- btreenodestructaddr btreenodestructaddr cellAddr )
    test_insertCell2                         ( btreenodestructaddr btreenodestructaddr cellAddr -- btreenodestructaddr )

    dup                                     ( btreenodestructaddr -- btreenodestructaddr btreenodestructaddr )
    test_getCell                            ( btreenodestructaddr btreenodestructaddr -- btreenodestructaddr )

    dup                                     ( btreenodestructaddr -- btreenodestructaddr btreenodestructaddr )
    allocateTableCellLeaf                   ( btreenodestructaddr btreenodestructaddr -- btreenodestructaddr btreenodestructaddr cellAddr )
    test_insertCell3                        ( btreenodestructaddr btreenodestructaddr cellAddr -- btreenodestructaddr )

    free
;

runTest
bye