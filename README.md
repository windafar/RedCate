# RedCate
search core implementation based on consistency hash

It's includ implementation of consistent-hash
and the ranking function base on TF-IDE

With add host, Indexing and Searching become very quickly 

IMPORTENT IN FOLLOWS:
This is a experimental project currently, Because I couldn't test it on any more machines.
In design, It can be used in large-scale enterprise searches
But I do't kown that the situation running in distributed environment

I test it just use some server unit in localhost, no more host, no more money

In addition to this, mongodb consume memory very much as cach for indexes,
But, that is greatly likely to change when add machine(with reducing of indexes)


Continuing...

First, Update Document: Helper.UploadDocument

Then, Bulid indexes: SercherServerBase.BuildSercherIndexToMongoDB

Search Finally.


next, I will find host for test 

to be continue, i hoped so..

真想看下在分布式环境中的表现，索引的时候本机读写满了都是1kb/1.5s,没机器好气啊