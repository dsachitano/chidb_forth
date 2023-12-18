s" chidb.fs" included

clearstack

assert-level 3

: test_insertCell { btreenodestructaddr cellAddr -- }
    \ cell with childPageNum 2, and key 1
    2 cellAddr tableCell_internal_setChildPageNum
    1 cellAddr tableCell_setKey

    btreenodestructaddr 0 cellAddr chidb_Btree_insertCell  ( btreeNodeAddr cellNum cellAddr -- )

    btreenodestructaddr chidb_Btree_writeNode

    \ validate numCells
    btreenodestructaddr btree_getNumCells
    assert( 1 = )

    btreenodestructaddr btree_getCellsOffset
    assert( 1024 8 - = )
;

: test_insertCell2 { btreenodestructaddr cellAddr -- }
    \ cell with childPageNum 3, and key 2
    3 cellAddr tableCell_internal_setChildPageNum
    2 cellAddr tableCell_setKey

    btreenodestructaddr 1 cellAddr chidb_Btree_insertCell  ( btreeNodeAddr cellNum cellAddr -- )

    btreenodestructaddr chidb_Btree_writeNode

    \ validate numCells
    btreenodestructaddr btree_getNumCells
    assert( 2 = )

    btreenodestructaddr btree_getCellsOffset
    assert( 1024 16 - = )

;

: runTest 
    \ initialize a DB
    s" insertCellTest.db" chidb_Btree_open  ( -- )
    1 chidb_Btree_getNodeByPage dup         ( -- btreenodestructaddr btreenodestructaddr)
    allocateTableCellInternal               ( btreenodestructaddr btreenodestructaddr -- btreenodestructaddr btreenodestructaddr cellAddr )
    test_insertCell                         ( btreenodestructaddr btreenodestructaddr cellAddr -- btreenodestructaddr )


    dup                                     ( btreenodestructaddr -- btreenodestructaddr btreenodestructaddr )
    allocateTableCellInternal               ( btreenodestructaddr btreenodestructaddr -- btreenodestructaddr btreenodestructaddr cellAddr )
    test_insertCell2                         ( btreenodestructaddr btreenodestructaddr cellAddr -- btreenodestructaddr )

    free
;

runTest
bye