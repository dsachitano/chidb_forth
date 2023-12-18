s" chidb.fs" included

clearstack

assert-level 3

: should_fileHeaderFmtString
    s" SQLite format 3" dup     \ dup the strlen
    1 block swap                \ get the addr of block1, and use the above strlen
    assert( str= )              \ make sure the strings match
;

: should_formatStrNullTerminated
    1 block 15 + c@             \ get the char at offset 15
    assert( 0= )                \ should be zero (null-terminated string)
;

\ load a db file
: test_chidb_Btree_open
    s" foo.db" chidb_Btree_open

    should_fileHeaderFmtString
    should_formatStrNullTerminated
;

: allTests
    test_chidb_Btree_open
;

allTests
bye