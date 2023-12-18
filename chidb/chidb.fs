\ ChiDb implementation, part 2
\ I started this on the dell, which died, so here we go again :)
\
\ based on http://chi.cs.uchicago.edu/chidb/assignment_btree.html#step-1-opening-a-chidb-file
\ and https://github.com/uchicago-cs/chidb
\

\ Step 1: Opening a chidb file
\ ---------------------------------------------------------------------
\ int chidb_Btree_open(const char *filename, chidb *db, BTree **bt)
\ int chidb_Btree_close(BTree *bt);
\ 
\ 
\ Step 2: Loading a B-Tree node from the file
\ ---------------------------------------------------------------------
\ int chidb_Btree_getNodeByPage(BTree *bt, npage_t npage, BTreeNode **node);
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