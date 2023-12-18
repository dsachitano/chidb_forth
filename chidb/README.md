## testing setup

For general testing, use the following command:

```
rm foo.db && gforth test.fs
```

Here's more notes on stuff I've done as part of testing:

```
 1115  rm insertCellTest.db && gforth testInsertCell.fs 
 1116  hexdump insertCellTest.db 
 1117  hexdump --help
 1118  hexdump -C insertCellTest.db 
 1119  hexdump -C -s 100 insertCellTest.db 
 1120  hexdump -s 100 insertCellTest.db 
 1121  hexdump -C insertCellTest.db 
 1122  hexdump -C -s 0x400 insertCellTest.db 
 1123  hexdump -C -s 0x464 insertCellTest.db 
 1124  ls
 1125  mkdir testFiles
 1126  mv ~/Downloads/strings-1page.sdb testFiles/
 1127  ls
 1128  hexdump -C testFiles/strings-1page.sdb 
 1129  hexdump -C -s 0x64 testFiles/strings-1page.sdb 
 1130  hexdump -C -s 0xd0 testFiles/strings-1page.sdb 
 1131  hexdump -C -s 0x134 testFiles/strings-1page.sdb 
 1132  hexdump -C -s 0x64 testFiles/strings-1page.sdb 
 1133  hexdump -C -s 0x268 testFiles/strings-1page.sdb 
 1134  hexdump -C -s 0x1e0 testFiles/strings-1page.sdb 
 1135  hexdump -C -s 0x158 testFiles/strings-1page.sdb 
 1136  hexdump -C -s 0x378 testFiles/strings-1page.sdb 
 1137  hexdump -C -s 0x0d0 testFiles/strings-1page.sdb 
 1138  hexdump -C -s 0x2f0 testFiles/strings-1page.sdb 
 1139  hexdump -C -s 0x464 insertCellTest.db 
 1140  hexdump -C -s 0x7e8 insertCellTest.db 
 1141  hexdump -C -s 0x7b8 insertCellTest.db 
 1142  hexdump -C -s 0x7d0 insertCellTest.db 
 1143  mv ~/Downloads/strings-1btree.sdb testFiles/
 1144  hexdump -C testFiles/strings-1btree.sdb 
 1145  hexdump -C -s 0x64 testFiles/strings-1btree.sdb 
 1146  hexdump -C -s 0x3f0 testFiles/strings-1btree.sdb 
 1147  hexdump -C -s 0x3f8 testFiles/strings-1btree.sdb 
 1148  hexdump -C -s 0xc00 testFiles/strings-1btree.sdb 
 1149  hexdump -C -s 0xf78 testFiles/strings-1btree.sdb 
 1150  hexdump -C -s 0xef0 testFiles/strings-1btree.sdb 
 1151  gforth testFindKey.fs 
 1152  hexdump -C -s 0x464 insertCellTest.db 
 1153  rm insertCellTest.db && gforth testInsertCell.fs 
 1154  hexdump -C -s 0x464 insertCellTest.db 
 1155  gforth testFindKey.fs 
 1156  hexdump -C -s 0x464 insertCellTest.db 
 1157  rm insertCellTest.db && gforth testInsertCell.fs 
 1158  hexdump -C -s 0x464 insertCellTest.db 
 1159  gforth testFindKey.fs 
 1160  hexdump -C -s 0x464 insertCellTest.db 
 1161  gforth testFindKey.fs 
 1162  git status
 1163  git add btreenodes.fs 
 1164  git add globalsAndConsts.fs 
 1165  git add testFiles/
 1166  git add testFindKey.fs 
 1167  git status
 1168  git diff --cached
 1169  git commit
 1170  git log
 1171  git push origin chidbBtrees
 1172  git log
 1173  cp testFiles/strings-1page.sdb chidbtest1.sdb
 1174  hexdump -C -s 0x64 testFiles/strings-1page.sdb 
 1175  gforth testFindKey.fs 
 1176  hexdump -C -s 0x64 chidbtest1.sdb
 1177  hexdump -C chidbtest1.sdb
 1178  gforth --version
 1179  gforth testFindKey.fs 
 1180  cp testFiles/strings-1btree.sdb chidbtest2.sdb
```