s" utils.fs" required

\ here, check block 1 to see if there's a file Header or not
: thereIsNoFileHeaderInBlock1 ( -- bool )
    1 \ just hard-code to non-zero for now, so we always will write file header
      \ TODO: actually check the block for a file header in the first 100 bytes 
;

: writeSqliteFormatStr ( -- )
    s" SQLite format 3" 1 block swap    \ produces (straddr blockaddr strlen -- )
    chars move  \ ( -- ) writes the string to 1 block at index 0

    0x0 1 block 15 + c!     \ get offset 15, and then write 0 into it
;

\ write the pageSize
: writePageSize ( -- )
    \ we know the offset where we'll write this 2-byte number, it's 0x10 
    \ according to the spec
    pageSize @          \ (pageSize -- )    get the pageSize from var cell
    1 block 0x10 +     \ (pageSize blockOffset -- )   get the block addr offset by 0x10
    2                   \ (pageSize blockOffset buffSize -- ) it's a 2-byte number
    writeMultiByteNum
;

\ not really sure what these are, but the spec has a sequence of
\ byte values for these offsets
: writeBytes12to17 
    0x01 1 block 0x12 + c!
    0x01 1 block 0x13 + c!
    0x00 1 block 0x14 + c!
    0x40 1 block 0x15 + c!
    0x20 1 block 0x16 + c!
    0x20 1 block 0x17 + c!
;

\ the fileChangeCounter is initialized to 0
\ and is a 4-byte number at offset 0x18
: writeFileChangeCounter
    0x0             \ (fileChangeCounter -- )   initialized to zero
    1 block 0x18 + \ (fileChangeCounter offsetAddr -- )
    4               \ (fileChangeCounter offsetAddr 4 -- )
    writeMultiByteNum
;

\ the spec says these 8 bytes are occupied by two separate
\ 4-byte numbers, each initialized to zero.  so all zeros, but
\ we'll just write them out as numbers instead of doing fill.
: writeBytes20to27
    0x00
    1 block 0x20 +
    4
    writeMultiByteNum   

    0x00
    1 block 0x24 +
    4
    writeMultiByteNum
;

: writeSchemaVersion ( version -- )
    1 block 0x28 +
    4
    writeMultiByteNum
;

\ the schema version is initialized to zero
: initializeSchemaVersion 
    0 writeSchemaVersion
;

\ spec says to write 0x1 as a 4 byte num at offset 0x2C
: writeBytes2Cto2F
    0x1
    1 block 0x2C +
    4
    writeMultiByteNum
;

: writePageCacheSize
    1 block 0x30 +
    4 writeMultiByteNum
;

\ spec says pageCacheSize initialized to 20000
: initializePageCacheSize
    20000 writePageCacheSize
;

\ spec says a 4-byte 0, then a 4-byte 1
: writeBytes34to3B
    0x0 1 block 0x34 + 4 writeMultiByteNum
    0x1 1 block 0x38 + 4 writeMultiByteNum
;

\ 4-byte num at 0x3C
: writeUserCookie ( val -- )
    1 block 0x3C + 4 writeMultiByteNum
;

\ spec says user cookie initialized to 0
: initializeUserCookie
    0x0 writeUserCookie
;

\ spec says write 4-byte 0 value here
: writeBytes40to43
    0x0 1 block 0x40 + 4 writeMultiByteNum
;

\ write the fileheader
: initializeFileHeader ( -- )
    writeSqliteFormatStr
    writePageSize
    writeBytes12to17
    writeFileChangeCounter
    \ bytes 1C to 1F are unused
    writeBytes20to27
    initializeSchemaVersion
    writeBytes2Cto2F
    initializePageCacheSize
    writeBytes34to3B
    initializeUserCookie
    writeBytes40to43
    \ bytes 0x44 to 0x63 are unused

;