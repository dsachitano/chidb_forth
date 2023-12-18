s" tablecells.fs" included

clearstack

assert-level 3

: validateTableCellInternal { cellAddr -- }
    cellAddr tableCell_getType
    assert( PGTYPE_TABLE_INTERNAL = )

    3 cellAddr tableCell_internal_setChildPageNum
    cellAddr tableCell_internal_getChildPageNum
    assert( 3 = )

    0x76230021 cellAddr tableCell_internal_setKey
    cellAddr tableCell_internal_getKey
    assert( 0x76230021 = )

    cellAddr 9 dump  
;

: test_gettersAndSetters 
    allocateTableCellInternal ( -- cellAddr )
    validateTableCellInternal
;

: test_allocateAndFree 
    allocateTableCellInternal ( -- cellAddr )
    dup 
    assert( 0 <> )

    dup 
    9 dump

    free 
;

: allTests
    clearstack test_allocateAndFree
    clearstack test_gettersAndSetters
;

allTests
bye