s" btreenodestruct.fs" included

clearstack

assert-level 3

: test_allocateAndFree
    allocateBtreeNode
    dup 
    assert( 0 <> )

    free 
;

: testPageNum ( nodeAddr -- )
    \ debug: dumpNode

    dup 
    4398 swap btree_setPageNum

    \ debug: dumpNode 
    btree_getPageNum
    assert( 4398 = )
;

: testPageType ( nodeAddr -- )

    dup
    PGTYPE_TABLE_LEAF swap btree_setPageType

    \ debug: dumpNode

    btree_getPageType
    assert( PGTYPE_TABLE_LEAF = )
;

: testFreeOffset ( nodeAddr -- )
    dup
    3207 swap btree_setFreeOffset
    \ dumpNode 
    btree_getFreeOffset
    assert( 3207 = )
;

: testNumCells ( nodeAddr -- )
    dup
    4 swap btree_setNumCells
    \ dumpNode 
    btree_getNumCells
    assert( 4 = )
;

: testCellsOffset ( nodeAddr -- )
    dup
    187 swap btree_setCellsOffset
    \ dumpNode 
    btree_getCellsOffset
    assert( 187 = )
;

: testRightPage ( nodeAddr -- )
    dup
    0x1B74AF0 swap btree_setRightPage
    \ dumpNode 
    btree_getRightPage
    assert( 0x1B74AF0 = )
;

: testCellOffsetArrayPtr ( nodeAddr -- )
    dup 
    0xB68D46F0 swap btree_setCellOffsetArrayPtr
    dumpNode
    btree_getCellOffsetArrayPtr
    assert( 0xB68D46F0 = )
;

: testEachSetterGetter { node -- }
    node testPageNum
    node testPageType
    node testFreeOffset
    node testNumCells
    node testCellsOffset
    node testRightPage
    node testCellOffsetArrayPtr
;

: test_settersGetters 
    allocateBtreeNode 
    testEachSetterGetter
;

: allTests
    clearstack test_allocateAndFree
    clearstack test_settersGetters
;

allTests
bye