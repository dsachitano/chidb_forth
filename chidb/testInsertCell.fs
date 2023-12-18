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

    free 
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

    free
;

runTest
bye