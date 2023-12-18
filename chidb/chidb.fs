s" utils.fs" required

\ ChiDb implementation, part 2
\ I started this on the dell, which died, so here we go again :)
\
\ based on http://chi.cs.uchicago.edu/chidb/assignment_btree.html#step-1-opening-a-chidb-file
\ and https://github.com/uchicago-cs/chidb
\

\ Variables:
\ ---------------------------------------------------------------------
Variable pageSize
1024 pageSize !


\ Step 1: Opening a chidb file
\ ---------------------------------------------------------------------

\ here, check block 1 to see if there's a file Header or not
: thereIsNoFileHeaderInBlock1 ( -- bool )
    1 \ just hard-code to non-zero for now, so we always will write file header
      \ TODO: actually check the block for a file header in the first 100 bytes 
;

: writeSqliteFormatStr ( -- )
    s" SQLite format 3" 1 buffer swap    \ produces (straddr blockaddr strlen -- )
    chars move  \ ( -- ) writes the string to 1 block at index 0

    0x0 1 buffer 15 + c!     \ get offset 15, and then write 0 into it
;

\ write the pageSize
: writePageSize ( -- )
    \ we know the offset where we'll write this 2-byte number, it's 0x10 
    \ according to the spec
    pageSize @          \ (pageSize -- )    get the pageSize from var cell
    1 buffer 0x10 +     \ (pageSize bufferOffset -- )   get the buffer addr offset by 0x10
    2                   \ (pageSize bufferOffset buffSize -- ) it's a 2-byte number
    writeMultiByteNum
;

\ not really sure what these are, but the spec has a sequence of
\ byte values for these offsets
: writeBytes12to17 
    0x01 1 buffer 0x12 + c!
    0x01 1 buffer 0x13 + c!
    0x00 1 buffer 0x14 + c!
    0x40 1 buffer 0x15 + c!
    0x20 1 buffer 0x16 + c!
    0x20 1 buffer 0x17 + c!
;

\ the fileChangeCounter is initialized to 0
\ and is a 4-byte number at offset 0x18
: writeFileChangeCounter
    0x0             \ (fileChangeCounter -- )   initialized to zero
    1 buffer 0x18 + \ (fileChangeCounter offsetAddr -- )
    4               \ (fileChangeCounter offsetAddr 4 -- )
    writeMultiByteNum
;

\ the spec says these 8 bytes are occupied by two separate
\ 4-byte numbers, each initialized to zero.  so all zeros, but
\ we'll just write them out as numbers instead of doing fill.
: writeBytes20to27
    0x00
    1 buffer 0x20 +
    4
    writeMultiByteNum   

    0x00
    1 buffer 0x24 +
    4
    writeMultiByteNum
;

: writeSchemaVersion ( version -- )
    1 buffer 0x28 +
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
    1 buffer 0x2C +
    4
    writeMultiByteNum
;

: writePageCacheSize
    1 buffer 0x30 +
    4 writeMultiByteNum
;

\ spec says pageCacheSize initialized to 20000
: initializePageCacheSize
    20000 writePageCacheSize
;

\ spec says a 4-byte 0, then a 4-byte 1
: writeBytes34to3B
    0x0 1 buffer 0x34 + 4 writeMultiByteNum
    0x1 1 buffer 0x38 + 4 writeMultiByteNum
;

\ 4-byte num at 0x3C
: writeUserCookie ( val -- )
    1 buffer 0x3C + 4 writeMultiByteNum
;

\ spec says user cookie initialized to 0
: initializeUserCookie
    0x0 writeUserCookie
;

\ spec says write 4-byte 0 value here
: writeBytes40to43
    0x0 1 buffer 0x40 + 4 writeMultiByteNum
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

\ int chidb_Btree_open(const char *filename, chidb *db, BTree **bt)
\ here, we're basically just taking a string in, and passing it to 
\ open-blocks, which uses the filename in the string as the blockfile
: chidb_Btree_open ( c-addr u -- )
    open-blocks

    thereIsNoFileHeaderInBlock1
    IF
        initializeFileHeader
    ENDIF

    update flush
;

\ int chidb_Btree_close(BTree *bt);
\ 
\ 
\ Step 2: Loading a B-Tree node from the file
\ ---------------------------------------------------------------------
\ int chidb_Btree_getNodeByPage(BTree *bt, npage_t npage, BTreeNode **node);
\ here, we'll just assume one btree (blockfile) at a time, and so then page
\ number is just a block number
: chidb_Btree_getNodeByPage ( pagenum -- )
    block
;

\ int chidb_Btree_freeMemNode(BTree *bt, BTreeNode *btn);
\
\ 
\ Step 3: Creating and writing a B-Tree node to disk
\ ---------------------------------------------------------------------
\ int chidb_Btree_newNode(BTree *bt, npage_t *npage, uint8_t type);
\ int chidb_Btree_initEmptyNode(BTree *bt, npage_t npage, uint8_t type);
\ int chidb_Btree_writeNode(BTree *bt, BTreeNode *node);
\
\ 
\ Step 4: Manipulating B-Tree cells
\ ---------------------------------------------------------------------
\ int chidb_Btree_getCell(BTreeNode *btn, ncell_t ncell, BTreeCell *cell);
\ int chidb_Btree_insertCell(BTreeNode *btn, ncell_t ncell, BTreeCell *cell);
\
\ 
\ Step 5: Finding a value in a B-Tree
\ ---------------------------------------------------------------------
\ int chidb_Btree_find(BTree *bt, npage_t nroot, key_t key, uint8_t **data, uint16_t *size);
\ 
\
\ Step 6: Insertion into a leaf without splitting
\ ---------------------------------------------------------------------
\ int chidb_Btree_insertInTable(BTree *bt, npage_t nroot, key_t key, uint8_t *data, uint16_t size);
\ int chidb_Btree_insert(BTree *bt, npage_t nroot, BTreeCell *btc);
\ int chidb_Btree_insertNonFull(BTree *bt, npage_t npage, BTreeCell *btc);
\
\
\ Step 7: Insertion with splitting
\ ---------------------------------------------------------------------
\ int chidb_Btree_split(BTree *bt, npage_t npage_parent, npage_t npage_child, ncell_t parent_cell, npage_t *npage_child2);
\
\
\ Step 8: Supporting index B-Trees
\ ---------------------------------------------------------------------
\ int chidb_Btree_insertInIndex(BTree *bt, npage_t nroot, key_t keyIdx, key_t keyPk);
\
\