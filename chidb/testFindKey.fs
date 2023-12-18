s" chidb.fs" included

clearstack

assert-level 3

: test_findKey 
    \ $00000001 0

    \ we use 7F as the key, which is 127, which should take us to value foo127 from page 0 (aka page 1 in chidb docs)
    $0000007F 0
    chidb_Btree_find ( keyVal4Byte pageNum -- dataAddr dataSize2Byte)

    \ 2dup dump lets us print out the memory in the address returned by the Btree_find
    2dup dump

    \ TODO assert that the string matches, not a 16= like here
    assert( 16 = )
;

: runTest 
    \ first open an existing db from disk.
    \ NOTE: this needs to use open-blocks, since chidb_Btree_open is really more
    \ about initializing a new database.  here, we're usingg "insertCellTest.db"
    \ which gets produced by running `gforth testInsertCell.fs` to add some cells to
    \ a B-tree.
    \ s" insertCellTest.db" open-blocks 


    \ This file contains a single B-Tree with height 2. The root node is located in page 1, and its child nodes are located in pages 2, 3, 4, and 5.
    \ Internal node (page 1)
    \ Printing Keys <= 7
    \ Leaf node (page 4)
    \     1 ->       foo1
    \     2 ->       foo2
    \     3 ->       foo3
    \     7 ->       foo7
    \ Printing Keys <= 35
    \ Leaf node (page 3)
    \    10 ->      foo10
    \    15 ->      foo15
    \    20 ->      foo20
    \    35 ->      foo35
    \ Printing Keys <= 1000
    \ Leaf node (page 5)
    \    37 ->      foo37
    \    42 ->      foo42
    \   127 ->     foo127
    \  1000 ->    foo1000
    \ Printing Keys > 1000
    \ Leaf node (page 2)
    \  2000 ->    foo2000
    \  3000 ->    foo3000
    \  4000 ->    foo4000
    \  5000 ->    foo5000
    s" chidbtest2.sdb" open-blocks 


    test_findKey


;

runTest
bye