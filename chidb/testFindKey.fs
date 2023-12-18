s" chidb.fs" included

clearstack

assert-level 3

: test_findKey 
    $00000002 1
    chidb_Btree_find ( keyVal4Byte pageNum -- dataAddr dataSize2Byte)
    assert( 16 = )
;

: runTest 
    \ first open an existing db from disk.
    \ NOTE: this needs to use open-blocks, since chidb_Btree_open is really more
    \ about initializing a new database.  here, we're usingg "insertCellTest.db"
    \ which gets produced by running `gforth testInsertCell.fs` to add some cells to
    \ a B-tree.
    s" insertCellTest.db" open-blocks 


    test_findKey


;

runTest
bye