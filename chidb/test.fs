s" chidb.fs" included

clearstack

assert-level 3

: should_fileHeaderFmtString
    s" SQLite format 3" dup     \ (strAddr strlen strlen -- ) dup the strlen
    1 block swap                \ (strAddr strlen blockAddr strlen -- ) get the addr of block1, and use the above strlen
    assert( str= )              \ make sure the strings match
;

: should_formatStrNullTerminated
    1 block 15 + c@             \ get the char at offset 15
    assert( 0= )                \ should be zero (null-terminated string)
;

: should_havePageSize1024
    1 block 0x10 +              \ (pageSizeAddr -- ) get the address of the pageSize
    2                           \ (pageSizeAddr 2 -- ) the pageSize is two bytes
    multiByteNum                \ (pageSize -- ) read the pageSize out of the two bytes
    assert( 1024 = )            \ expect the pageSize to be 1024
;

\ load a db file
: test_chidb_Btree_open
    s" foo.db" chidb_Btree_open

    should_fileHeaderFmtString
    should_formatStrNullTerminated
    \ should_havePageSize1024
;

create testBuf 4 allot      \ dictionary allocate a 4-byte buffer called testBuf

: should_multiByteNum_addsUp
    0x1 testBuf !
    0x2 testBuf 1 + !
    0x3 testBuf 2 + !
    0x4 testBuf 3 + ! 

    clearstack 
    testBuf 4 multiByteNum 
    assert( 0x1 0x2 0x3 0x4 + + + = )
;

: should_multiByteNum_biggerThan256
    0x0 testBuf !
    0x0 testBuf 1 + !
    0x1 testBuf 2 + !
    0x0 testBuf 3 + !

    clearstack 
    testBuf 4 multiByteNum 
    assert( 0x100 = )
;

: should_multiByteNum_biggerThan256_twoBytes
    0x0 testBuf !
    0x0 testBuf 1 + !
    0x1 testBuf 2 + !
    0x20 testBuf 3 + !

    clearstack 
    testBuf 4 multiByteNum 
    assert( 0x120 = )
;

: should_multiByteNum_oneByte
    0x0 testBuf !
    0x0 testBuf 1 + !
    0x0 testBuf 2 + !
    0x15 testBuf 3 + !

    clearstack 
    testBuf 4 multiByteNum 
    assert( 0x15 = )
;

: should_multiByteNum_threeByte
    0x0 testBuf !
    0x03 testBuf 1 + !
    0x25 testBuf 2 + !
    0x1A testBuf 3 + !

    clearstack 
    testBuf 4 multiByteNum 
    assert( 0x3251A = )
;

: should_multiByteNum_fourByte
    0x07 testBuf !
    0x20 testBuf 1 + !
    0x13 testBuf 2 + !
    0xAF testBuf 3 + !

    clearstack 
    testBuf 4 multiByteNum 
    assert( 0x72013AF = )
;

: test_utils
    should_multiByteNum_biggerThan256
    should_multiByteNum_biggerThan256_twoBytes
    should_multiByteNum_oneByte
    should_multiByteNum_threeByte
    should_multiByteNum_fourByte
;

: allTests
    test_chidb_Btree_open
    test_utils
;

allTests
bye