s" tablecells.fs" included

clearstack

assert-level 3

: validateTableCellInternal { cellAddr -- }
    cellAddr tableCell_getType
    assert( PGTYPE_TABLE_INTERNAL = )

    3 cellAddr tableCell_internal_setChildPageNum
    cellAddr tableCell_internal_getChildPageNum
    assert( 3 = )

    0x76230021 cellAddr tableCell_setKey
    cellAddr tableCell_getKey
    assert( 0x76230021 = )

    \ the size of the in memory cell obj should be 9
    cellAddr tableCell_getSize
    assert( 9 = )

    \ the size of the corresponding cell when written to block should be 8
    cellAddr tableCell_getBlockSize
    assert( 8 = )
;

: test_gettersAndSetters 
    allocateTableCellInternal ( -- cellAddr ) dup
    validateTableCellInternal
    free 
;

: test_allocateAndFree 
    allocateTableCellInternal ( -- cellAddr )
    dup 
    assert( 0 <> )

    \ dup 
    \ 9 dump

    free 
;

: validateTableCellLeaf { cellAddr -- }
    cellAddr tableCell_getType
    assert( PGTYPE_TABLE_LEAF = )

    0x4521 cellAddr tableCell_setKey
    cellAddr tableCell_getKey
    assert( 0x4521 = )

    56 cellAddr tableCell_leaf_setRecordSize
    cellAddr tableCell_leaf_getRecordSize
    assert( 56 = )

    cellAddr tableCell_getSize
    assert( 13 = )

    cellAddr tableCell_getBlockSize
    assert( 8 56 + = )
;

: test_leafCell 
    allocateTableCellLeaf dup 
    validateTableCellLeaf
    free
;

: allTests
    clearstack test_allocateAndFree
    clearstack test_gettersAndSetters
    clearstack test_leafCell
;

allTests
bye