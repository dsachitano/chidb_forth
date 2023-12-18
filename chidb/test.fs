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

: pageHeader_should_match_type
    clearstack 
    1 block 0x64 + 
    c@
    
    assert( PGTYPE_TABLE_LEAF = )
;

\ since we initialize the page header as type PGTYPE_TABLE_LEAF
\ that means the header doesn't have a right page pointer in the last 4
\ bytes, so the header is really only 8 bytes long, so check that here
: pageHeader_should_have_initialFreeOffset 
    1 block 0x64 +      \ base addr of pageHeader
    1 +                 \ offset for freeOffset
    2 multiByteNum      \ 2 byte number
    assert( 8 = )       \ should be 8
;

: pageHeader_should_have_zeroNumCells
    1 block 0x64 +      \ base addr of pageHeader
    3 +                 \ offset for numCells
    2 multiByteNum      \ 2 byte number
    assert( 0 = )       \ should be zero
;

: pageHeader_should_have_pageSizeForCellsOffset
    1 block 0x64 +      \ base addr of pageHeader
    5 +                 \ offset for numCells
    2 multiByteNum      \ 2 byte number
    assert( 1024 = )    \ should be pageSize (hardcoded here to 1024, could be `pageSize @` instead)
;

: pageHeader_should_have_zeroAtOffset7 
    1 block 0x64 +      \ base addr of pageHeader
    7 +                 \ offset for numCells
    c@                  \ 1 byte number
    assert( 0 = )       \ should be 0
;

: should_havePageHeader
    pageHeader_should_match_type
    pageHeader_should_have_initialFreeOffset
    pageHeader_should_have_zeroNumCells
    pageHeader_should_have_pageSizeForCellsOffset
    pageHeader_should_have_zeroAtOffset7
;

\ load a db file
: test_chidb_Btree_open
    s" foo.db" chidb_Btree_open

    clearstack 

    should_fileHeaderFmtString
    should_formatStrNullTerminated
    should_havePageSize1024
    should_havePageHeader
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

: should_writeMultiByteNum_oneByte
    testBuf 4 blank     \ initialize testBuf to 32s
    0x12 testBuf 4 writeMultiByteNum    \ write 0x12 into testBuf of 4 bytes

    assert( testBuf 0 + c@ 0x0 = )     
    assert( testBuf 1 + c@ 0x0 = )
    assert( testBuf 2 + c@ 0x0 = )
    assert( testBuf 3 + c@ 0x12 = )
;

: should_writeMultiByteNum_twoByte
    testBuf 4 blank     \ initialize testBuf to 32s
    0x2312 testBuf 4 writeMultiByteNum    \ write 0x2312 into testBuf of 4 bytes
        
    assert( testBuf 0 + c@ 0x0 = )     
    assert( testBuf 1 + c@ 0x0 = )
    assert( testBuf 2 + c@ 0x23 = )
    assert( testBuf 3 + c@ 0x12 = )
;

: should_writeMultiByteNum_threeByte
    testBuf 4 blank     \ initialize testBuf to 32s
    0xFA2312 testBuf 4 writeMultiByteNum    \ write 0xFA2312 into testBuf of 4 bytes
        
    assert( testBuf 0 + c@ 0x0 = )     
    assert( testBuf 1 + c@ 0xFA = )
    assert( testBuf 2 + c@ 0x23 = )
    assert( testBuf 3 + c@ 0x12 = )
;

: should_writeMultiByteNum_threeByte_withBlanks
    testBuf 4 blank     \ initialize testBuf to 32s
    0x810001 testBuf 4 writeMultiByteNum    \ write 0xFA2312 into testBuf of 4 bytes
        
    assert( testBuf 0 + c@ 0x0 = )     
    assert( testBuf 1 + c@ 0x81 = )
    assert( testBuf 2 + c@ 0x00 = )
    assert( testBuf 3 + c@ 0x01 = )
;

: should_writeMultiByteNum_fourByte
    testBuf 4 blank     \ initialize testBuf to 32s
    0x10FA2312 testBuf 4 writeMultiByteNum    \ write 0x10FA2312 into testBuf of 4 bytes
        
    assert( testBuf 0 + c@ 0x10 = )     
    assert( testBuf 1 + c@ 0xFA = )
    assert( testBuf 2 + c@ 0x23 = )
    assert( testBuf 3 + c@ 0x12 = )
;

: should_roundTrip_multiByte 
    testBuf 4 blank     \ initialize testBuf to 32s

    \ first write a multibyte num
    0x45F3 testBuf 4 writeMultiByteNum

    \ then read it
    testBuf 4 multiByteNum 
    assert( 0x45F3 = )
;

: test_utils
    should_multiByteNum_biggerThan256
    should_multiByteNum_biggerThan256_twoBytes
    should_multiByteNum_oneByte
    should_multiByteNum_threeByte
    should_multiByteNum_fourByte

    should_writeMultiByteNum_oneByte
    should_writeMultiByteNum_twoByte
    should_writeMultiByteNum_threeByte
    should_writeMultiByteNum_threeByte_withBlanks
    should_writeMultiByteNum_fourByte

    should_roundTrip_multiByte
;

: test_constants
    assert( PGTYPE_TABLE_INTERNAL 0x05 = )
    assert( 0x0D PGTYPE_TABLE_LEAF = )
    assert( 0x02 PGTYPE_INDEX_INTERNAL = )
    assert( 0x0A PGTYPE_INDEX_LEAF = )
;

: allTests
    clearstack test_constants
    clearstack test_chidb_Btree_open
    clearstack test_utils
;

allTests
bye